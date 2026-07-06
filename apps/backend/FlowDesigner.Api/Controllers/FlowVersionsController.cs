using System.Text.Json;
using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Versions;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Application.Security;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/flows/{flowId:guid}/versions")]
[Authorize]
public sealed class FlowVersionsController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(PermissionCodes.VersionRead)]
    public async Task<ActionResult<IReadOnlyList<FlowVersionSummaryDto>>> List(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flowExists = await dbContext.Flows.AnyAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (!flowExists)
        {
            return ApiError.NotFound<IReadOnlyList<FlowVersionSummaryDto>>(this, "Flow was not found.");
        }

        var versions = await dbContext.Versions
            .AsNoTracking()
            .Include(x => x.CreatedByUser)
            .Where(x => x.FlowId == flowId)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        var summaries = versions.Select(version =>
        {
            using var document = JsonDocument.Parse(version.SnapshotJson);
            var root = document.RootElement;
            var nodeCount = CountArray(root, "nodes");
            var linkCount = CountArray(root, "links");
            var commentCount = CountArray(root, "comments");

            return new FlowVersionSummaryDto(
                version.VersionId,
                version.FlowId,
                version.VersionNumber,
                $"v{version.VersionNumber}",
                version.Note,
                version.CreatedByUser?.DisplayName,
                version.CreatedAtUtc,
                nodeCount,
                linkCount,
                commentCount);
        }).ToList();

        return Ok(summaries);
    }

    [HttpGet("compare")]
    [RequirePermission(PermissionCodes.VersionRead)]
    public async Task<ActionResult<FlowVersionCompareResponse>> Compare(
        Guid projectId,
        Guid flowId,
        [FromQuery] Guid leftVersionId,
        [FromQuery] Guid rightVersionId,
        CancellationToken cancellationToken)
    {
        var flowExists = await dbContext.Flows.AnyAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (!flowExists)
        {
            return ApiError.NotFound<FlowVersionCompareResponse>(this, "Flow was not found.");
        }

        var leftVersion = await dbContext.Versions.AsNoTracking().FirstOrDefaultAsync(x => x.FlowId == flowId && x.VersionId == leftVersionId, cancellationToken);
        var rightVersion = await dbContext.Versions.AsNoTracking().FirstOrDefaultAsync(x => x.FlowId == flowId && x.VersionId == rightVersionId, cancellationToken);

        if (leftVersion is null || rightVersion is null)
        {
            return ApiError.NotFound<FlowVersionCompareResponse>(this, "One or more versions were not found.");
        }

        using var leftDocument = JsonDocument.Parse(leftVersion.SnapshotJson);
        using var rightDocument = JsonDocument.Parse(rightVersion.SnapshotJson);

        var leftRoot = leftDocument.RootElement;
        var rightRoot = rightDocument.RootElement;

        var leftLanes = DeserializeList<Lane>(leftRoot, "lanes");
        var rightLanes = DeserializeList<Lane>(rightRoot, "lanes");
        var leftStages = DeserializeList<Stage>(leftRoot, "stages");
        var rightStages = DeserializeList<Stage>(rightRoot, "stages");
        var leftNodes = DeserializeList<FlowNode>(leftRoot, "nodes");
        var rightNodes = DeserializeList<FlowNode>(rightRoot, "nodes");
        var leftLinks = DeserializeList<FlowLink>(leftRoot, "links");
        var rightLinks = DeserializeList<FlowLink>(rightRoot, "links");
        var leftComments = DeserializeList<FlowComment>(leftRoot, "comments");
        var rightComments = DeserializeList<FlowComment>(rightRoot, "comments");

        return Ok(new FlowVersionCompareResponse(
            leftVersionId,
            rightVersionId,
            CompareLanes(leftLanes, rightLanes),
            CompareStages(leftStages, rightStages),
            CompareNodes(leftNodes, rightNodes),
            CompareLinks(leftLinks, rightLinks),
            CompareComments(leftComments, rightComments)));
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.VersionCreate)]
    public async Task<ActionResult<FlowVersionSummaryDto>> Create(Guid projectId, Guid flowId, [FromBody] CreateFlowVersionRequest request, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);

        if (flow is null)
        {
            return ApiError.NotFound<FlowVersionSummaryDto>(this, "Flow was not found.");
        }

        var currentUserId = currentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        var currentUser = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

        var lanes = await dbContext.Lanes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var stages = await dbContext.Stages.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var nodes = await dbContext.Nodes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var links = await dbContext.Links.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var comments = await dbContext.Comments.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var metadata = await dbContext.MetadataItems.AsNoTracking().Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);

        var nextVersionNumber = await dbContext.Versions
            .Where(x => x.FlowId == flowId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var snapshot = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            flow = new
            {
                flow.FlowId,
                flow.ProjectId,
                flow.Name,
                flow.Description,
                flow.SortOrder,
                flow.Revision,
            },
            lanes,
            stages,
            nodes,
            links,
            comments,
            metadata,
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var version = new FlowVersion
        {
            VersionId = Guid.NewGuid(),
            FlowId = flowId,
            VersionNumber = nextVersionNumber + 1,
            CreatedByUserId = currentUserId,
            SnapshotJson = snapshot,
            Note = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        dbContext.Versions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new FlowVersionSummaryDto(
            version.VersionId,
            version.FlowId,
            version.VersionNumber,
            $"v{version.VersionNumber}",
            version.Note,
            currentUser?.DisplayName,
            version.CreatedAtUtc,
            nodes.Count,
            links.Count,
            comments.Count));
    }

    [HttpPost("{versionId:guid}/restore")]
    [RequirePermission(PermissionCodes.VersionCreate)]
    public async Task<ActionResult<RestoreFlowVersionResponse>> Restore(Guid projectId, Guid flowId, Guid versionId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.FlowId == flowId, cancellationToken);
        if (flow is null)
        {
            return ApiError.NotFound<RestoreFlowVersionResponse>(this, "Flow was not found.");
        }

        var version = await dbContext.Versions.FirstOrDefaultAsync(x => x.FlowId == flowId && x.VersionId == versionId, cancellationToken);
        if (version is null)
        {
            return ApiError.NotFound<RestoreFlowVersionResponse>(this, "Version was not found.");
        }

        var currentUserId = currentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var backupVersion = await CreateSnapshotVersionAsync(flow, currentUserId, "Auto backup before restore", cancellationToken);

        using var document = JsonDocument.Parse(version.SnapshotJson);
        var root = document.RootElement;

        var restoredLanes = root.GetProperty("lanes").Deserialize<List<Lane>>() ?? [];
        var restoredStages = root.GetProperty("stages").Deserialize<List<Stage>>() ?? [];
        var restoredNodes = root.GetProperty("nodes").Deserialize<List<FlowNode>>() ?? [];
        var restoredLinks = root.GetProperty("links").Deserialize<List<FlowLink>>() ?? [];
        var restoredComments = root.GetProperty("comments").Deserialize<List<FlowComment>>() ?? [];
        var restoredMetadata = root.TryGetProperty("metadata", out var metadataElement)
            ? metadataElement.Deserialize<List<FlowMetadata>>() ?? []
            : [];

        var existingLanes = await dbContext.Lanes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingStages = await dbContext.Stages.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingNodes = await dbContext.Nodes.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingLinks = await dbContext.Links.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingComments = await dbContext.Comments.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);
        var existingMetadata = await dbContext.MetadataItems.Where(x => x.FlowId == flowId).ToListAsync(cancellationToken);

        dbContext.Comments.RemoveRange(existingComments);
        dbContext.Links.RemoveRange(existingLinks);
        dbContext.Nodes.RemoveRange(existingNodes);
        dbContext.Stages.RemoveRange(existingStages);
        dbContext.Lanes.RemoveRange(existingLanes);
        dbContext.MetadataItems.RemoveRange(existingMetadata);

        foreach (var lane in restoredLanes)
        {
            lane.FlowId = flowId;
        }

        foreach (var stage in restoredStages)
        {
            stage.FlowId = flowId;
        }

        foreach (var node in restoredNodes)
        {
            node.FlowId = flowId;
        }

        foreach (var link in restoredLinks)
        {
            link.FlowId = flowId;
        }

        foreach (var comment in restoredComments)
        {
            comment.FlowId = flowId;
        }

        foreach (var metadata in restoredMetadata)
        {
            metadata.FlowId = flowId;
        }

        dbContext.Lanes.AddRange(restoredLanes);
        dbContext.Stages.AddRange(restoredStages);
        dbContext.Nodes.AddRange(restoredNodes);
        dbContext.Links.AddRange(restoredLinks);
        dbContext.Comments.AddRange(restoredComments);
        dbContext.MetadataItems.AddRange(restoredMetadata);

        flow.Revision += 1;
        flow.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new RestoreFlowVersionResponse(flow.FlowId, backupVersion.VersionId, flow.Revision));
    }

    private async Task<FlowVersion> CreateSnapshotVersionAsync(Flow flow, Guid? createdByUserId, string? comment, CancellationToken cancellationToken)
    {
        var lanes = await dbContext.Lanes.AsNoTracking().Where(x => x.FlowId == flow.FlowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var stages = await dbContext.Stages.AsNoTracking().Where(x => x.FlowId == flow.FlowId).OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var nodes = await dbContext.Nodes.AsNoTracking().Where(x => x.FlowId == flow.FlowId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var links = await dbContext.Links.AsNoTracking().Where(x => x.FlowId == flow.FlowId).ToListAsync(cancellationToken);
        var comments = await dbContext.Comments.AsNoTracking().Where(x => x.FlowId == flow.FlowId).ToListAsync(cancellationToken);
        var metadata = await dbContext.MetadataItems.AsNoTracking().Where(x => x.FlowId == flow.FlowId).ToListAsync(cancellationToken);

        var nextVersionNumber = await dbContext.Versions
            .Where(x => x.FlowId == flow.FlowId)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var snapshot = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            flow = new
            {
                flow.FlowId,
                flow.ProjectId,
                flow.Name,
                flow.Description,
                flow.SortOrder,
                flow.Revision,
            },
            lanes,
            stages,
            nodes,
            links,
            comments,
            metadata,
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var version = new FlowVersion
        {
            VersionId = Guid.NewGuid(),
            FlowId = flow.FlowId,
            VersionNumber = nextVersionNumber + 1,
            CreatedByUserId = createdByUserId,
            SnapshotJson = snapshot,
            Note = comment,
            CreatedAtUtc = DateTime.UtcNow,
        };

        dbContext.Versions.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);
        return version;
    }

    private static int CountArray(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;
    }

    private static List<T> DeserializeList<T>(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var element)
            ? element.Deserialize<List<T>>() ?? []
            : [];
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareLanes(IReadOnlyList<Lane> left, IReadOnlyList<Lane> right)
    {
        return CompareById(
            "Lane",
            left.ToDictionary(x => x.LaneId, x => x),
            right.ToDictionary(x => x.LaneId, x => x),
            lane => lane.Name,
            (l, r) => l.Name != r.Name || l.SortOrder != r.SortOrder);
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareStages(IReadOnlyList<Stage> left, IReadOnlyList<Stage> right)
    {
        return CompareById(
            "Stage",
            left.ToDictionary(x => x.StageId, x => x),
            right.ToDictionary(x => x.StageId, x => x),
            stage => stage.Name,
            (l, r) => l.Name != r.Name || l.SortOrder != r.SortOrder);
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareNodes(IReadOnlyList<FlowNode> left, IReadOnlyList<FlowNode> right)
    {
        var diffs = new List<VersionDiffItemDto>();
        var leftMap = left.ToDictionary(x => x.NodeId, x => x);
        var rightMap = right.ToDictionary(x => x.NodeId, x => x);

        foreach (var added in rightMap.Keys.Except(leftMap.Keys))
        {
            var node = rightMap[added];
            diffs.Add(new VersionDiffItemDto("Node", node.NodeId.ToString(), "Added", node.Name));
        }

        foreach (var deleted in leftMap.Keys.Except(rightMap.Keys))
        {
            var node = leftMap[deleted];
            diffs.Add(new VersionDiffItemDto("Node", node.NodeId.ToString(), "Deleted", node.Name));
        }

        foreach (var common in leftMap.Keys.Intersect(rightMap.Keys))
        {
            var leftNode = leftMap[common];
            var rightNode = rightMap[common];

            if (leftNode.Name != rightNode.Name ||
                leftNode.Description != rightNode.Description ||
                leftNode.NodeType != rightNode.NodeType ||
                leftNode.LaneId != rightNode.LaneId ||
                leftNode.StageId != rightNode.StageId)
            {
                diffs.Add(new VersionDiffItemDto("Node", rightNode.NodeId.ToString(), "Updated", rightNode.Name));
            }
            else if (leftNode.X != rightNode.X || leftNode.Y != rightNode.Y)
            {
                diffs.Add(new VersionDiffItemDto("Node", rightNode.NodeId.ToString(), "Moved", rightNode.Name));
            }
        }

        return diffs;
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareLinks(IReadOnlyList<FlowLink> left, IReadOnlyList<FlowLink> right)
    {
        return CompareById(
            "Link",
            left.ToDictionary(x => x.LinkId, x => x),
            right.ToDictionary(x => x.LinkId, x => x),
            link => link.Label ?? link.Condition ?? link.LinkId.ToString(),
            (l, r) => l.Label != r.Label || l.Condition != r.Condition || l.SourceNodeId != r.SourceNodeId || l.TargetNodeId != r.TargetNodeId);
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareComments(IReadOnlyList<FlowComment> left, IReadOnlyList<FlowComment> right)
    {
        return CompareById(
            "Comment",
            left.ToDictionary(x => x.CommentId, x => x),
            right.ToDictionary(x => x.CommentId, x => x),
            comment => comment.Text,
            (l, r) => l.Text != r.Text || l.NodeId != r.NodeId || l.X != r.X || l.Y != r.Y);
    }

    private static IReadOnlyList<VersionDiffItemDto> CompareById<T>(
        string entityType,
        IReadOnlyDictionary<Guid, T> left,
        IReadOnlyDictionary<Guid, T> right,
        Func<T, string> labelSelector,
        Func<T, T, bool> updatedPredicate)
    {
        var diffs = new List<VersionDiffItemDto>();

        foreach (var added in right.Keys.Except(left.Keys))
        {
            var item = right[added];
            diffs.Add(new VersionDiffItemDto(entityType, added.ToString(), "Added", labelSelector(item)));
        }

        foreach (var deleted in left.Keys.Except(right.Keys))
        {
            var item = left[deleted];
            diffs.Add(new VersionDiffItemDto(entityType, deleted.ToString(), "Deleted", labelSelector(item)));
        }

        foreach (var common in left.Keys.Intersect(right.Keys))
        {
            var leftItem = left[common];
            var rightItem = right[common];
            if (updatedPredicate(leftItem, rightItem))
            {
                diffs.Add(new VersionDiffItemDto(entityType, common.ToString(), "Updated", labelSelector(rightItem)));
            }
        }

        return diffs;
    }
}
