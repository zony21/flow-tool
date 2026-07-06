using System.Text.Json;
using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Versions;
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
public sealed class FlowVersionsController(AppDbContext dbContext) : ControllerBase
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

    private static int CountArray(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;
    }
}
