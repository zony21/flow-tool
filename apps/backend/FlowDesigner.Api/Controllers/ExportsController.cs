using System.Text;
using System.Text.Json;
using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Exports;
using FlowDesigner.Application.Security;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/flows/{flowId:guid}/export")]
[Authorize]
public sealed class ExportsController(AppDbContext dbContext) : ControllerBase
{
    private static readonly JsonSerializerOptions ExportJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    [HttpPost("json")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<IActionResult> ExportJson(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Flow was not found."));
        }

        var json = JsonSerializer.Serialize(snapshot, ExportJsonOptions);
        return File(
            Encoding.UTF8.GetBytes(json),
            "application/json",
            $"{BuildFileName(snapshot.ProjectName, snapshot.FlowName, "json")}.json");
    }

    [HttpPost("mermaid")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportMermaid(
        Guid projectId,
        Guid flowId,
        [FromBody] MermaidExportRequest? request,
        CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null)
        {
            return ApiError.NotFound<TextExportResponse>(this, "Flow was not found.");
        }

        var type = string.IsNullOrWhiteSpace(request?.Type) ? "flowchart" : request.Type.Trim();
        if (!string.Equals(type, "flowchart", StringComparison.OrdinalIgnoreCase))
        {
            return ApiError.BadRequest<TextExportResponse>(this, "Only Mermaid flowchart export is currently supported.", "type");
        }

        var direction = string.IsNullOrWhiteSpace(request?.Direction) ? "TD" : request.Direction.Trim().ToUpperInvariant();
        if (direction is not ("TD" or "TB" or "BT" or "LR" or "RL"))
        {
            return ApiError.BadRequest<TextExportResponse>(this, "Mermaid direction must be TD, TB, BT, LR, or RL.", "direction");
        }

        var content = BuildMermaidFlowchart(snapshot, direction, request?.IncludeComments ?? true);
        return Ok(new TextExportResponse(
            $"{BuildFileName(snapshot.ProjectName, snapshot.FlowName, "flowchart")}.mmd",
            content));
    }

    private async Task<FlowSnapshot?> BuildSnapshotAsync(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.FlowId == flowId)
            .Select(x => new
            {
                x.FlowId,
                x.ProjectId,
                ProjectName = x.Project.Name,
                FlowName = x.Name,
                x.Description,
                x.SortOrder,
                x.Revision,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (flow is null)
        {
            return null;
        }

        var lanes = await dbContext.Lanes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.LaneId,
                x.FlowId,
                x.Name,
                x.SortOrder,
            })
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.StageId,
                x.FlowId,
                x.Name,
                x.SortOrder,
            })
            .ToListAsync(cancellationToken);

        var nodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.NodeId,
                x.FlowId,
                x.LaneId,
                x.StageId,
                x.NodeType,
                x.Name,
                x.Description,
                x.X,
                x.Y,
            })
            .ToListAsync(cancellationToken);

        var links = await dbContext.Links.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new
            {
                x.LinkId,
                x.FlowId,
                x.SourceNodeId,
                x.TargetNodeId,
                x.Label,
                x.Condition,
            })
            .ToListAsync(cancellationToken);

        var comments = await dbContext.Comments.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new
            {
                x.CommentId,
                x.FlowId,
                x.NodeId,
                x.Text,
                x.X,
                x.Y,
            })
            .ToListAsync(cancellationToken);

        var metadata = await dbContext.MetadataItems.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new
            {
                x.MetadataId,
                x.FlowId,
                x.MetaKey,
                x.MetaValue,
            })
            .ToListAsync(cancellationToken);

        return new FlowSnapshot(
            1,
            DateTime.UtcNow,
            new ExportProject(flow.ProjectId, flow.ProjectName),
            new ExportFlow(
                flow.FlowId,
                flow.ProjectId,
                flow.FlowName,
                flow.Description,
                flow.SortOrder,
                flow.Revision,
                flow.CreatedAtUtc,
                flow.UpdatedAtUtc),
            lanes.Select(x => new ExportLane(x.LaneId, x.FlowId, x.Name, x.SortOrder)).ToList(),
            stages.Select(x => new ExportStage(x.StageId, x.FlowId, x.Name, x.SortOrder)).ToList(),
            nodes.Select(x => new ExportNode(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y)).ToList(),
            links.Select(x => new ExportLink(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition)).ToList(),
            comments.Select(x => new ExportComment(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y)).ToList(),
            metadata.Select(x => new ExportMetadata(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue)).ToList());
    }

    private static string BuildMermaidFlowchart(FlowSnapshot snapshot, string direction, bool includeComments)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"flowchart {direction}");
        builder.AppendLine($"%% Flow: {EscapeMermaidComment(snapshot.Flow.Name)}");

        var nodeIds = new Dictionary<Guid, string>();
        var index = 1;
        foreach (var node in snapshot.Nodes)
        {
            var nodeKey = $"N{index++}";
            nodeIds[node.NodeId] = nodeKey;
            builder.AppendLine($"    {nodeKey}[\"{EscapeMermaidLabel(node.Name)}\"]");
        }

        foreach (var link in snapshot.Links)
        {
            if (!nodeIds.TryGetValue(link.SourceNodeId, out var source) || !nodeIds.TryGetValue(link.TargetNodeId, out var target))
            {
                continue;
            }

            var label = string.IsNullOrWhiteSpace(link.Label) ? null : EscapeMermaidLabel(link.Label);
            builder.AppendLine(label is null
                ? $"    {source} --> {target}"
                : $"    {source} -->|\"{label}\"| {target}");
        }

        if (includeComments)
        {
            foreach (var comment in snapshot.Comments)
            {
                builder.AppendLine($"    %% Comment: {EscapeMermaidComment(comment.Text)}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string EscapeMermaidLabel(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
    }

    private static string EscapeMermaidComment(string value)
    {
        return value.Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
    }

    private static string BuildFileName(string projectName, string flowName, string exportType)
    {
        return $"{SafeFilePart(projectName)}_{SafeFilePart(flowName)}_{exportType}_{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static string SafeFilePart(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Trim())
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        var filePart = builder.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(filePart) ? "flow" : filePart;
    }

    private sealed record FlowSnapshot(
        int SchemaVersion,
        DateTime ExportedAtUtc,
        ExportProject Project,
        ExportFlow Flow,
        IReadOnlyList<ExportLane> Lanes,
        IReadOnlyList<ExportStage> Stages,
        IReadOnlyList<ExportNode> Nodes,
        IReadOnlyList<ExportLink> Links,
        IReadOnlyList<ExportComment> Comments,
        IReadOnlyList<ExportMetadata> Metadata)
    {
        public string ProjectName => Project.Name;
        public string FlowName => Flow.Name;
    }

    private sealed record ExportProject(Guid ProjectId, string Name);

    private sealed record ExportFlow(
        Guid FlowId,
        Guid ProjectId,
        string Name,
        string? Description,
        int SortOrder,
        int CurrentRevision,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record ExportLane(Guid LaneId, Guid FlowId, string Name, int SortOrder);

    private sealed record ExportStage(Guid StageId, Guid FlowId, string Name, int SortOrder);

    private sealed record ExportNode(
        Guid NodeId,
        Guid FlowId,
        Guid? LaneId,
        Guid? StageId,
        string NodeType,
        string Name,
        string? Description,
        double X,
        double Y);

    private sealed record ExportLink(
        Guid LinkId,
        Guid FlowId,
        Guid SourceNodeId,
        Guid TargetNodeId,
        string? Label,
        string? Condition);

    private sealed record ExportComment(
        Guid CommentId,
        Guid FlowId,
        Guid? NodeId,
        string Text,
        double X,
        double Y);

    private sealed record ExportMetadata(Guid MetadataId, Guid FlowId, string MetaKey, string MetaValue);
}
