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

    [HttpDelete("{flowId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<IActionResult> Delete(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Flow was not found."));
        }

        dbContext.Flows.Remove(flow);
        await dbContext.SaveChangesAsync(cancellationToken);
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
}
