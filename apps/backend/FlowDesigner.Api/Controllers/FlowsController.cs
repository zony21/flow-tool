using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Flows;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Application.Security;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/flows")]
[Authorize]
public sealed class FlowsController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    IPermissionService permissionService) : ControllerBase
{
    private const double StageWidth = 240;
    private const double NodeOffsetX = 42;

    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [HttpGet]
    [RequirePermission(PermissionCodes.FlowRead)]
    public async Task<ActionResult<IReadOnlyList<FlowSummaryDto>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var flows = await dbContext.Flows
            .AsNoTracking()
            .Where(flow => flow.ProjectId == projectId)
            .OrderBy(flow => flow.SortOrder)
            .ThenBy(flow => flow.CreatedAtUtc)
            .Select(flow => new FlowSummaryDto(
                flow.FlowId,
                flow.ProjectId,
                flow.Name,
                flow.Description,
                flow.SortOrder,
                flow.CreatedAtUtc,
                flow.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(flows);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<FlowDetailDto>> Create(Guid projectId, [FromBody] CreateFlowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiError.BadRequest<FlowDetailDto>(this, "Flow name is required.", "name");
        }

        var projectExists = await dbContext.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken);
        if (!projectExists)
        {
            return ApiError.NotFound<FlowDetailDto>(this, "Project was not found.");
        }

        var sortOrder = await dbContext.Flows
            .Where(flow => flow.ProjectId == projectId)
            .Select(flow => (int?)flow.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var now = DateTime.UtcNow;
        var flow = new Flow
        {
            FlowId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Description = request.Description,
            SortOrder = sortOrder + 1,
            Revision = 0,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        dbContext.Flows.Add(flow);
        dbContext.Lanes.Add(new Lane
        {
            LaneId = Guid.NewGuid(),
            FlowId = flow.FlowId,
            Name = "担当レーン",
            SortOrder = 1,
        });
        dbContext.Stages.Add(new Stage
        {
            StageId = Guid.NewGuid(),
            FlowId = flow.FlowId,
            Name = "工程",
            SortOrder = 1,
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = await BuildDetailDtoAsync(projectId, flow.FlowId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId, flowId = flow.FlowId }, detail);
    }

    [HttpGet("{flowId:guid}")]
    [RequirePermission(PermissionCodes.FlowRead)]
    public async Task<ActionResult<FlowDetailDto>> Get(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);

        if (flow is null)
        {
            return ApiError.NotFound<FlowDetailDto>(this, "Flow was not found.");
        }

        return Ok(await BuildDetailDtoAsync(projectId, flowId, cancellationToken));
    }

    [HttpPut("{flowId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<FlowDetailDto>> Update(Guid projectId, Guid flowId, [FromBody] UpdateFlowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiError.BadRequest<FlowDetailDto>(this, "Flow name is required.", "name");
        }

        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return ApiError.NotFound<FlowDetailDto>(this, "Flow was not found.");
        }

        flow.Name = request.Name.Trim();
        flow.Description = request.Description;
        flow.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(await BuildDetailDtoAsync(projectId, flowId, cancellationToken));
    }

    [HttpPut("{flowId:guid}/structure")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<SaveFlowStructureResponse>> SaveStructure(Guid projectId, Guid flowId, [FromBody] SaveFlowStructureRequest request, CancellationToken cancellationToken)
    {
        if (request.FlowId != flowId)
        {
            return ApiError.BadRequest<SaveFlowStructureResponse>(this, "FlowId in path and body must match.", "flowId");
        }

        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return ApiError.NotFound<SaveFlowStructureResponse>(this, "Flow was not found.");
        }

        var currentUserId = currentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        var canUpdateFlow = await permissionService.CanAsync(currentUserId.Value, projectId, PermissionCodes.FlowUpdate, cancellationToken);
        if (!canUpdateFlow)
        {
            return StatusCode(403, ApiError.Create(HttpContext, ApiErrorCodes.Forbidden, "FlowUpdate permission is required."));
        }

        if (request.ClientRevision != flow.Revision)
        {
            return ApiError.Conflict<SaveFlowStructureResponse>(this, ApiErrorCodes.RevisionConflict, "Flow structure has changed.");
        }

        var lanes = (request.Lanes ?? Array.Empty<SaveLaneRequest>()).ToList();
        var stages = (request.Stages ?? Array.Empty<SaveStageRequest>()).ToList();
        var nodes = NormalizeNodeAssignments((request.Nodes ?? Array.Empty<SaveNodeRequest>()).ToList(), lanes, stages);
        var links = (request.Links ?? Array.Empty<SaveLinkRequest>()).ToList();
        var comments = (request.Comments ?? Array.Empty<SaveCommentRequest>()).ToList();

        var validationError = ValidateStructure(lanes, stages, nodes, links, comments);
        if (validationError is not null)
        {
            return UnprocessableEntity(ApiError.Create(
                HttpContext,
                ApiErrorCodes.ValidationError,
                validationError,
                ApiError.ValidationDetails("structure", validationError)));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingLanes = await dbContext.Lanes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingStages = await dbContext.Stages.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingNodes = await dbContext.Nodes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingLinks = await dbContext.Links.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingComments = await dbContext.Comments.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);

        dbContext.Comments.RemoveRange(existingComments);
        dbContext.Links.RemoveRange(existingLinks);
        dbContext.Nodes.RemoveRange(existingNodes);
        dbContext.Stages.RemoveRange(existingStages);
        dbContext.Lanes.RemoveRange(existingLanes);

        dbContext.Lanes.AddRange(lanes.Select(lane => new Lane
        {
            LaneId = lane.LaneId,
            FlowId = flowId,
            Name = lane.Name.Trim(),
            SortOrder = lane.SortOrder,
        }));

        dbContext.Stages.AddRange(stages.Select(stage => new Stage
        {
            StageId = stage.StageId,
            FlowId = flowId,
            Name = stage.Name.Trim(),
            SortOrder = stage.SortOrder,
        }));

        dbContext.Nodes.AddRange(nodes.Select(node => new FlowNode
        {
            NodeId = node.NodeId,
            FlowId = flowId,
            LaneId = node.LaneId,
            StageId = node.StageId,
            NodeType = node.NodeType.Trim(),
            Name = node.Name.Trim(),
            Description = node.Description,
            X = node.X,
            Y = node.Y,
        }));

        dbContext.Links.AddRange(links.Select(link => new FlowLink
        {
            LinkId = link.LinkId,
            FlowId = flowId,
            SourceNodeId = link.SourceNodeId,
            TargetNodeId = link.TargetNodeId,
            Label = link.Label,
            Condition = link.Condition,
        }));

        dbContext.Comments.AddRange(comments.Select(comment => new FlowComment
        {
            CommentId = comment.CommentId,
            FlowId = flowId,
            NodeId = comment.NodeId,
            Text = comment.Text.Trim(),
            X = comment.X,
            Y = comment.Y,
        }));

        if (request.CreateVersion)
        {
            var nextVersionNumber = await dbContext.Versions
                .Where(x => x.FlowId == flowId)
                .Select(x => (int?)x.VersionNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var snapshot = JsonSerializer.Serialize(new
            {
                flowId,
                request.ClientRevision,
                lanes,
                stages,
                nodes,
                links,
                comments,
                request.ChangeSummary,
            }, SnapshotJsonOptions);

            dbContext.Versions.Add(new FlowVersion
            {
                VersionId = Guid.NewGuid(),
                FlowId = flowId,
                VersionNumber = nextVersionNumber + 1,
                CreatedByUserId = currentUserId,
                SnapshotJson = snapshot,
                Note = request.ChangeSummary,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        flow.Revision += 1;
        flow.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new SaveFlowStructureResponse(flow.FlowId, flow.Revision, flow.UpdatedAtUtc));
    }

    [HttpPost("{flowId:guid}/duplicate")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<FlowDetailDto>> Duplicate(
        Guid projectId,
        Guid flowId,
        [FromBody] DuplicateFlowRequest? request,
        CancellationToken cancellationToken)
    {
        var sourceFlow = await dbContext.Flows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);

        if (sourceFlow is null)
        {
            return ApiError.NotFound<FlowDetailDto>(this, "Flow was not found.");
        }

        var currentUserId = currentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        var now = DateTime.UtcNow;
        var nextSortOrder = await dbContext.Flows
            .Where(flow => flow.ProjectId == projectId)
            .Select(flow => (int?)flow.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var requestedName = request?.Name?.Trim();
        var duplicateName = await BuildDuplicateFlowNameAsync(
            projectId,
            string.IsNullOrWhiteSpace(requestedName) ? $"{sourceFlow.Name} Copy" : requestedName,
            cancellationToken);

        var duplicateFlow = new Flow
        {
            FlowId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = duplicateName,
            Description = sourceFlow.Description,
            SortOrder = nextSortOrder + 1,
            Revision = 0,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var sourceLanes = await dbContext.Lanes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var sourceStages = await dbContext.Stages.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var sourceNodes = await dbContext.Nodes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var sourceLinks = await dbContext.Links.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var sourceComments = await dbContext.Comments.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var sourceMetadata = await dbContext.MetadataItems.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);

        var laneIdMap = sourceLanes.ToDictionary(x => x.LaneId, _ => Guid.NewGuid());
        var stageIdMap = sourceStages.ToDictionary(x => x.StageId, _ => Guid.NewGuid());
        var nodeIdMap = sourceNodes.ToDictionary(x => x.NodeId, _ => Guid.NewGuid());

        duplicateFlow.Revision = sourceFlow.Revision;
        dbContext.Flows.Add(duplicateFlow);
        dbContext.Lanes.AddRange(sourceLanes.Select(lane => new Lane
        {
            LaneId = laneIdMap[lane.LaneId],
            FlowId = duplicateFlow.FlowId,
            Name = lane.Name,
            SortOrder = lane.SortOrder,
        }));
        dbContext.Stages.AddRange(sourceStages.Select(stage => new Stage
        {
            StageId = stageIdMap[stage.StageId],
            FlowId = duplicateFlow.FlowId,
            Name = stage.Name,
            SortOrder = stage.SortOrder,
        }));
        dbContext.Nodes.AddRange(sourceNodes.Select(node => new FlowNode
        {
            NodeId = nodeIdMap[node.NodeId],
            FlowId = duplicateFlow.FlowId,
            LaneId = node.LaneId.HasValue && laneIdMap.ContainsKey(node.LaneId.Value) ? laneIdMap[node.LaneId.Value] : null,
            StageId = node.StageId.HasValue && stageIdMap.ContainsKey(node.StageId.Value) ? stageIdMap[node.StageId.Value] : null,
            NodeType = node.NodeType,
            Name = node.Name,
            Description = node.Description,
            X = node.X,
            Y = node.Y,
        }));
        dbContext.Links.AddRange(sourceLinks.Where(link => nodeIdMap.ContainsKey(link.SourceNodeId) && nodeIdMap.ContainsKey(link.TargetNodeId)).Select(link => new FlowLink
        {
            LinkId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            SourceNodeId = nodeIdMap[link.SourceNodeId],
            TargetNodeId = nodeIdMap[link.TargetNodeId],
            Label = link.Label,
            Condition = link.Condition,
        }));
        dbContext.Comments.AddRange(sourceComments.Select(comment => new FlowComment
        {
            CommentId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            NodeId = comment.NodeId.HasValue && nodeIdMap.ContainsKey(comment.NodeId.Value) ? nodeIdMap[comment.NodeId.Value] : null,
            Text = comment.Text,
            X = comment.X,
            Y = comment.Y,
        }));
        dbContext.MetadataItems.AddRange(sourceMetadata.Select(metadata => new FlowMetadata
        {
            MetadataId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            MetaKey = metadata.MetaKey,
            MetaValue = metadata.MetaValue,
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(await BuildDetailDtoAsync(projectId, duplicateFlow.FlowId, cancellationToken));
    }

    private async Task<FlowDetailDto> BuildDetailDtoAsync(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.AsNoTracking().FirstAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        var lanes = await dbContext.Lanes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).Select(x => new LaneDto(x.LaneId, x.FlowId, x.Name, x.SortOrder)).ToListAsync(cancellationToken);
        var stages = await dbContext.Stages.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).Select(x => new StageDto(x.StageId, x.FlowId, x.Name, x.SortOrder)).ToListAsync(cancellationToken);
        var rawNodes = await dbContext.Nodes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.Name).Select(x => new NodeDto(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y)).ToListAsync(cancellationToken);
        var nodes = NormalizeNodeDetailDtos(rawNodes, lanes, stages);
        var links = await dbContext.Links.AsNoTracking().Where(x => x.FlowId == flowId).Select(x => new LinkDto(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition)).ToListAsync(cancellationToken);
        var comments = await dbContext.Comments.AsNoTracking().Where(x => x.FlowId == flowId).Select(x => new CommentDto(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y)).ToListAsync(cancellationToken);
        var metadata = await dbContext.MetadataItems.AsNoTracking().Where(x => x.FlowId == flowId).Select(x => new MetadataDto(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue)).ToListAsync(cancellationToken);

        return new FlowDetailDto(flow.FlowId, flow.ProjectId, flow.Name, flow.Description, flow.SortOrder, flow.Revision, lanes, stages, nodes, links, comments, metadata);
    }

    private async Task<string> BuildDuplicateFlowNameAsync(Guid projectId, string baseName, CancellationToken cancellationToken)
    {
        var existingNames = await dbContext.Flows
            .AsNoTracking()
            .Where(flow => flow.ProjectId == projectId && flow.Name.StartsWith(baseName))
            .Select(flow => flow.Name)
            .ToListAsync(cancellationToken);

        if (!existingNames.Contains(baseName))
        {
            return baseName;
        }

        var suffix = 2;
        while (existingNames.Contains($"{baseName} {suffix}"))
        {
            suffix++;
        }

        return $"{baseName} {suffix}";
    }

    private static IReadOnlyList<NodeDto> NormalizeNodeDetailDtos(
        IReadOnlyList<NodeDto> nodes,
        IReadOnlyList<LaneDto> lanes,
        IReadOnlyList<StageDto> stages)
    {
        var sortedLanes = lanes.OrderBy(lane => lane.SortOrder).ToList();
        var sortedStages = stages.OrderBy(stage => stage.SortOrder).ToList();
        var laneIds = sortedLanes.Select(lane => lane.LaneId).ToHashSet();
        var stageIds = sortedStages.Select(stage => stage.StageId).ToHashSet();

        return nodes.Select(node =>
        {
            var laneId = node.LaneId.HasValue && laneIds.Contains(node.LaneId.Value)
                ? node.LaneId
                : sortedLanes.FirstOrDefault()?.LaneId;

            var stageId = node.StageId.HasValue && stageIds.Contains(node.StageId.Value)
                ? node.StageId
                : ResolveStageIdByX(node.X, sortedStages);

            return node with
            {
                LaneId = laneId,
                StageId = stageId,
            };
        }).ToList();
    }

    private static IReadOnlyList<SaveNodeRequest> NormalizeNodeAssignments(
        IReadOnlyList<SaveNodeRequest> nodes,
        IReadOnlyList<SaveLaneRequest> lanes,
        IReadOnlyList<SaveStageRequest> stages)
    {
        var sortedLanes = lanes.OrderBy(lane => lane.SortOrder).ToList();
        var sortedStages = stages.OrderBy(stage => stage.SortOrder).ToList();
        var laneIds = sortedLanes.Select(lane => lane.LaneId).ToHashSet();
        var stageIds = sortedStages.Select(stage => stage.StageId).ToHashSet();

        return nodes.Select(node =>
        {
            var laneId = node.LaneId.HasValue && laneIds.Contains(node.LaneId.Value)
                ? node.LaneId
                : sortedLanes.FirstOrDefault()?.LaneId;

            var stageId = node.StageId.HasValue && stageIds.Contains(node.StageId.Value)
                ? node.StageId
                : ResolveStageIdByX(node.X, sortedStages);

            return node with
            {
                LaneId = laneId,
                StageId = stageId,
            };
        }).ToList();
    }

    private static Guid? ResolveStageIdByX(double x, IReadOnlyList<SaveStageRequest> sortedStages)
    {
        if (sortedStages.Count == 0)
        {
            return null;
        }

        var index = (int)Math.Round((x - NodeOffsetX) / StageWidth);
        index = Math.Max(0, Math.Min(index, sortedStages.Count - 1));
        return sortedStages[index].StageId;
    }

    private static Guid? ResolveStageIdByX(double x, IReadOnlyList<StageDto> sortedStages)
    {
        if (sortedStages.Count == 0)
        {
            return null;
        }

        var index = (int)Math.Round((x - NodeOffsetX) / StageWidth);
        index = Math.Max(0, Math.Min(index, sortedStages.Count - 1));
        return sortedStages[index].StageId;
    }

    private static string? ValidateStructure(
        IReadOnlyList<SaveLaneRequest> lanes,
        IReadOnlyList<SaveStageRequest> stages,
        IReadOnlyList<SaveNodeRequest> nodes,
        IReadOnlyList<SaveLinkRequest> links,
        IReadOnlyList<SaveCommentRequest> comments)
    {
        if (lanes.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Lane name is required.";
        }

        if (stages.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Stage name is required.";
        }

        if (nodes.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Node name is required.";
        }

        var laneIds = lanes.Select(x => x.LaneId).ToHashSet();
        if (nodes.Any(x => x.LaneId.HasValue && !laneIds.Contains(x.LaneId.Value)))
        {
            return "Node laneId must reference an existing lane.";
        }

        var stageIds = stages.Select(x => x.StageId).ToHashSet();
        if (nodes.Any(x => x.StageId.HasValue && !stageIds.Contains(x.StageId.Value)))
        {
            return "Node stageId must reference an existing stage.";
        }

        var nodeIds = nodes.Select(x => x.NodeId).ToHashSet();
        if (links.Any(x => !nodeIds.Contains(x.SourceNodeId) || !nodeIds.Contains(x.TargetNodeId)))
        {
            return "Link source and target must reference existing nodes.";
        }

        if (comments.Any(x => x.NodeId.HasValue && !nodeIds.Contains(x.NodeId.Value)))
        {
            return "Comment nodeId must reference existing node.";
        }

        return null;
    }
}
