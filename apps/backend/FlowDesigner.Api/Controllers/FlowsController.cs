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

        var requiredPermissions = new[]
        {
            PermissionCodes.FlowUpdate,
            PermissionCodes.NodeUpdate,
            PermissionCodes.LinkUpdate,
            PermissionCodes.CommentUpdate,
        };

        foreach (var permission in requiredPermissions)
        {
            var allowed = await permissionService.CanAsync(currentUserId.Value, projectId, permission, cancellationToken);
            if (!allowed)
            {
                return StatusCode(403, ApiError.Create(HttpContext, ApiErrorCodes.Forbidden, $"{permission} permission is required."));
            }
        }

        if (request.ClientRevision != flow.Revision)
        {
            return ApiError.Conflict<SaveFlowStructureResponse>(this, ApiErrorCodes.RevisionConflict, "Flow structure has changed.");
        }

        var lanes = (request.Lanes ?? Array.Empty<SaveLaneRequest>()).ToList();
        var stages = (request.Stages ?? Array.Empty<SaveStageRequest>()).ToList();
        var nodes = (request.Nodes ?? Array.Empty<SaveNodeRequest>()).ToList();
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
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

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

        var duplicateLanes = sourceLanes.Select(lane => new Lane
        {
            LaneId = laneIdMap[lane.LaneId],
            FlowId = duplicateFlow.FlowId,
            Name = lane.Name,
            SortOrder = lane.SortOrder,
        }).ToList();

        var duplicateStages = sourceStages.Select(stage => new Stage
        {
            StageId = stageIdMap[stage.StageId],
            FlowId = duplicateFlow.FlowId,
            Name = stage.Name,
            SortOrder = stage.SortOrder,
        }).ToList();

        var duplicateNodes = sourceNodes.Select(node => new FlowNode
        {
            NodeId = nodeIdMap[node.NodeId],
            FlowId = duplicateFlow.FlowId,
            LaneId = node.LaneId is not null && laneIdMap.TryGetValue(node.LaneId.Value, out var laneId) ? laneId : null,
            StageId = node.StageId is not null && stageIdMap.TryGetValue(node.StageId.Value, out var stageId) ? stageId : null,
            NodeType = node.NodeType,
            Name = node.Name,
            Description = node.Description,
            X = node.X,
            Y = node.Y,
        }).ToList();

        var duplicateLinks = sourceLinks
            .Where(link => nodeIdMap.ContainsKey(link.SourceNodeId) && nodeIdMap.ContainsKey(link.TargetNodeId))
            .Select(link => new FlowLink
            {
                LinkId = Guid.NewGuid(),
                FlowId = duplicateFlow.FlowId,
                SourceNodeId = nodeIdMap[link.SourceNodeId],
                TargetNodeId = nodeIdMap[link.TargetNodeId],
                Label = link.Label,
                Condition = link.Condition,
            })
            .ToList();

        var duplicateComments = sourceComments.Select(comment => new FlowComment
        {
            CommentId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            NodeId = comment.NodeId is not null && nodeIdMap.TryGetValue(comment.NodeId.Value, out var nodeId) ? nodeId : null,
            Text = comment.Text,
            X = comment.X,
            Y = comment.Y,
        }).ToList();

        var duplicateMetadata = sourceMetadata.Select(metadata => new FlowMetadata
        {
            MetadataId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            MetaKey = metadata.MetaKey,
            MetaValue = metadata.MetaValue,
        }).ToList();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.Flows.Add(duplicateFlow);
        dbContext.Lanes.AddRange(duplicateLanes);
        dbContext.Stages.AddRange(duplicateStages);
        dbContext.Nodes.AddRange(duplicateNodes);
        dbContext.Links.AddRange(duplicateLinks);
        dbContext.Comments.AddRange(duplicateComments);
        dbContext.MetadataItems.AddRange(duplicateMetadata);

        dbContext.Versions.Add(new FlowVersion
        {
            VersionId = Guid.NewGuid(),
            FlowId = duplicateFlow.FlowId,
            VersionNumber = 1,
            CreatedByUserId = currentUserId,
            SnapshotJson = JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                flow = new
                {
                    duplicateFlow.FlowId,
                    duplicateFlow.ProjectId,
                    duplicateFlow.Name,
                    duplicateFlow.Description,
                    duplicateFlow.SortOrder,
                    duplicateFlow.Revision,
                },
                lanes = duplicateLanes.Select(lane => new
                {
                    lane.LaneId,
                    lane.FlowId,
                    lane.Name,
                    lane.SortOrder,
                }),
                stages = duplicateStages.Select(stage => new
                {
                    stage.StageId,
                    stage.FlowId,
                    stage.Name,
                    stage.SortOrder,
                }),
                nodes = duplicateNodes.Select(node => new
                {
                    node.NodeId,
                    node.FlowId,
                    node.LaneId,
                    node.StageId,
                    node.NodeType,
                    node.Name,
                    node.Description,
                    node.X,
                    node.Y,
                }),
                links = duplicateLinks.Select(link => new
                {
                    link.LinkId,
                    link.FlowId,
                    link.SourceNodeId,
                    link.TargetNodeId,
                    link.Label,
                    link.Condition,
                }),
                comments = duplicateComments.Select(comment => new
                {
                    comment.CommentId,
                    comment.FlowId,
                    comment.NodeId,
                    comment.Text,
                    comment.X,
                    comment.Y,
                }),
                metadata = duplicateMetadata.Select(metadata => new
                {
                    metadata.MetadataId,
                    metadata.FlowId,
                    metadata.MetaKey,
                    metadata.MetaValue,
                }),
            }, SnapshotJsonOptions),
            Note = $"Duplicated from {sourceFlow.Name}",
            CreatedAtUtc = now,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var detail = await BuildDetailDtoAsync(projectId, duplicateFlow.FlowId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId, flowId = duplicateFlow.FlowId }, detail);
    }

    [HttpDelete("{flowId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<IActionResult> Delete(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Flow was not found."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var images = await dbContext.Images.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        foreach (var image in images)
        {
            image.FlowId = null;
        }

        dbContext.Comments.RemoveRange(await dbContext.Comments.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Links.RemoveRange(await dbContext.Links.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.MetadataItems.RemoveRange(await dbContext.MetadataItems.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Versions.RemoveRange(await dbContext.Versions.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Nodes.RemoveRange(await dbContext.Nodes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Stages.RemoveRange(await dbContext.Stages.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Lanes.RemoveRange(await dbContext.Lanes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken));
        dbContext.Flows.Remove(flow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }

    private async Task<FlowDetailDto> BuildDetailDtoAsync(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.AsNoTracking().FirstAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        var lanes = await dbContext.Lanes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new LaneDto(x.LaneId, x.FlowId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new StageDto(x.StageId, x.FlowId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var nodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.Name)
            .Select(x => new NodeDto(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var links = await dbContext.Links.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new LinkDto(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition))
            .ToListAsync(cancellationToken);

        var comments = await dbContext.Comments.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new CommentDto(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var metadata = await dbContext.MetadataItems.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new MetadataDto(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue))
            .ToListAsync(cancellationToken);

        return new FlowDetailDto(
            flow.FlowId,
            flow.ProjectId,
            flow.Name,
            flow.Description,
            flow.SortOrder,
            flow.Revision,
            lanes,
            stages,
            nodes,
            links,
            comments,
            metadata);
    }

    private static string? ValidateStructure(
        IReadOnlyList<SaveLaneRequest> lanes,
        IReadOnlyList<SaveStageRequest> stages,
        IReadOnlyList<SaveNodeRequest> nodes,
        IReadOnlyList<SaveLinkRequest> links,
        IReadOnlyList<SaveCommentRequest> comments)
    {
        if (lanes.GroupBy(x => x.SortOrder).Any(group => group.Count() > 1))
        {
            return "Lane display order must be unique.";
        }

        if (stages.GroupBy(x => x.SortOrder).Any(group => group.Count() > 1))
        {
            return "Stage display order must be unique.";
        }

        if (lanes.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Lane name is required.";
        }

        if (stages.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Stage name is required.";
        }

        if (nodes.Any(x => string.IsNullOrWhiteSpace(x.NodeType) || string.IsNullOrWhiteSpace(x.Name)))
        {
            return "Node type and name are required.";
        }

        if (nodes.Any(x => x.X is double.NaN or double.PositiveInfinity or double.NegativeInfinity || x.Y is double.NaN or double.PositiveInfinity or double.NegativeInfinity))
        {
            return "Node coordinates must be finite numbers.";
        }

        var laneIds = lanes.Select(x => x.LaneId).ToHashSet();
        var stageIds = stages.Select(x => x.StageId).ToHashSet();
        var nodeIds = nodes.Select(x => x.NodeId).ToHashSet();

        if (nodes.Any(x => x.LaneId is not null && !laneIds.Contains(x.LaneId.Value)))
        {
            return "Node lane must exist in the same flow.";
        }

        if (nodes.Any(x => x.StageId is not null && !stageIds.Contains(x.StageId.Value)))
        {
            return "Node stage must exist in the same flow.";
        }

        if (links.Any(x => !nodeIds.Contains(x.SourceNodeId) || !nodeIds.Contains(x.TargetNodeId)))
        {
            return "Link endpoints must exist in the same flow.";
        }

        if (comments.Any(x => x.NodeId is not null && !nodeIds.Contains(x.NodeId.Value)))
        {
            return "Comment node must exist in the same flow.";
        }

        return null;
    }

    private async Task<string> BuildDuplicateFlowNameAsync(Guid projectId, string baseName, CancellationToken cancellationToken)
    {
        var existingNames = await dbContext.Flows
            .AsNoTracking()
            .Where(flow => flow.ProjectId == projectId)
            .Select(flow => flow.Name)
            .ToListAsync(cancellationToken);

        var existing = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseName))
        {
            return baseName;
        }

        for (var index = 2; ; index++)
        {
            var candidate = $"{baseName} {index}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }
    }
}
