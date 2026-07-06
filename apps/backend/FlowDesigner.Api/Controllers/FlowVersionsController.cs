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
                version.CreatedAtUtc,
                nodeCount,
                linkCount,
                commentCount);
        }).ToList();

        return Ok(summaries);
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

        var backupVersion = await CreateSnapshotVersionAsync(flow, "Auto backup before restore", cancellationToken);

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

    private async Task<FlowVersion> CreateSnapshotVersionAsync(Flow flow, string? comment, CancellationToken cancellationToken)
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
}
