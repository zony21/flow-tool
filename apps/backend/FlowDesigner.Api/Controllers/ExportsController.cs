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
    private const double StageWidth = 240;
    private const double NodeOffsetX = 42;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    [HttpPost("json")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<IActionResult> ExportJson(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null) return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Flow was not found."));
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        return File(Encoding.UTF8.GetBytes(json), "application/json", $"{Safe(snapshot.Project.Name)}_{Safe(snapshot.Flow.Name)}_json.json");
    }

    [HttpPost("mermaid")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportMermaid(Guid projectId, Guid flowId, [FromBody] MermaidExportRequest? request, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null) return ApiError.NotFound<TextExportResponse>(this, "Flow was not found.");
        var direction = string.IsNullOrWhiteSpace(request?.Direction) ? "TD" : request.Direction.Trim().ToUpperInvariant();
        if (direction is not ("TD" or "TB" or "BT" or "LR" or "RL")) return ApiError.BadRequest<TextExportResponse>(this, "Mermaid direction must be TD, TB, BT, LR, or RL.", "direction");
        return Ok(new TextExportResponse($"{Safe(snapshot.Project.Name)}_{Safe(snapshot.Flow.Name)}_flowchart.mmd", BuildMermaid(snapshot, direction)));
    }

    [HttpPost("ai-dsl")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportAiDsl(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null) return ApiError.NotFound<TextExportResponse>(this, "Flow was not found.");
        return Ok(new TextExportResponse($"{Safe(snapshot.Project.Name)}_{Safe(snapshot.Flow.Name)}_ai_dsl.flowdsl.txt", BuildAiDsl(snapshot)));
    }

    private async Task<FlowSnapshot?> BuildSnapshotAsync(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows.AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.FlowId == flowId)
            .Select(x => new ExportFlow(x.FlowId, x.ProjectId, x.Project.Name, x.Name, x.Description, x.SortOrder, x.Revision, x.CreatedAtUtc, x.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
        if (flow is null) return null;

        var lanes = await dbContext.Lanes.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder)
            .Select(x => new ExportLane(x.LaneId, x.FlowId, x.Name, x.SortOrder)).ToListAsync(cancellationToken);
        var stages = await dbContext.Stages.AsNoTracking().Where(x => x.FlowId == flowId).OrderBy(x => x.SortOrder)
            .Select(x => new ExportStage(x.StageId, x.FlowId, x.Name, x.Category, x.StageType, x.SortOrder)).ToListAsync(cancellationToken);
        var rawNodes = await dbContext.Nodes.AsNoTracking().Where(x => x.FlowId == flowId)
            .Select(x => new ExportNode(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y, x.CommandId, x.LocationId, x.ManufacturerVehicleTypeId, x.RwType)).ToListAsync(cancellationToken);
        var stageIds = stages.Select(x => x.StageId).ToHashSet();
        var nodes = rawNodes.Select(node => node with { StageId = node.StageId.HasValue && stageIds.Contains(node.StageId.Value) ? node.StageId : ResolveStageIdByX(node.X, stages) }).ToList();
        var links = await dbContext.Links.AsNoTracking().Where(x => x.FlowId == flowId)
            .Select(x => new ExportLink(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition)).ToListAsync(cancellationToken);
        var comments = await dbContext.Comments.AsNoTracking().Where(x => x.FlowId == flowId)
            .Select(x => new ExportComment(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y)).ToListAsync(cancellationToken);
        var metadata = await dbContext.MetadataItems.AsNoTracking().Where(x => x.FlowId == flowId)
            .Select(x => new ExportMetadata(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue)).ToListAsync(cancellationToken);

        return new FlowSnapshot(2, DateTime.UtcNow, new ExportProject(flow.ProjectId, flow.ProjectName), flow, lanes, stages, nodes, links, comments, metadata);
    }

    private static string BuildMermaid(FlowSnapshot snapshot, string direction)
    {
        var builder = new StringBuilder($"flowchart {direction}\n");
        var keys = snapshot.Nodes.Select((node, index) => (node.NodeId, Key: $"N{index + 1}")).ToDictionary(x => x.NodeId, x => x.Key);
        foreach (var stage in snapshot.Stages.OrderBy(x => x.SortOrder))
        {
            var stageNodes = snapshot.Nodes.Where(x => x.StageId == stage.StageId).ToList();
            if (stageNodes.Count == 0) continue;
            builder.AppendLine($"    subgraph S{stage.SortOrder}[\"{Escape(stage.Name)} ({Escape(stage.Category)})\"]");
            foreach (var node in stageNodes) builder.AppendLine($"        {keys[node.NodeId]}[\"{Escape(node.Name)}\"]");
            builder.AppendLine("    end");
        }
        foreach (var link in snapshot.Links)
        {
            if (!keys.TryGetValue(link.SourceNodeId, out var source) || !keys.TryGetValue(link.TargetNodeId, out var target)) continue;
            var label = string.IsNullOrWhiteSpace(link.Condition) ? link.Label : link.Condition;
            builder.AppendLine(string.IsNullOrWhiteSpace(label) ? $"    {source} --> {target}" : $"    {source} -->|\"{Escape(label!)}\"| {target}");
        }
        return builder.ToString().TrimEnd();
    }

    private static string BuildAiDsl(FlowSnapshot snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("AI_FLOW_DSL_VERSION: 2");
        builder.AppendLine("FLOW:");
        builder.AppendLine($"  project_name: {Value(snapshot.Project.Name)}");
        builder.AppendLine($"  flow_name: {Value(snapshot.Flow.Name)}");
        builder.AppendLine("STAGES:");
        foreach (var stage in snapshot.Stages.OrderBy(x => x.SortOrder))
        {
            builder.AppendLine($"  - stage_id: {stage.StageId}");
            builder.AppendLine($"    name: {Value(stage.Name)}");
            builder.AppendLine($"    category: {stage.Category}");
            builder.AppendLine($"    stage_type: {stage.StageType}");
            builder.AppendLine($"    sort_order: {stage.SortOrder}");
        }
        builder.AppendLine("NODES:");
        foreach (var node in snapshot.Nodes)
        {
            var stage = snapshot.Stages.FirstOrDefault(x => x.StageId == node.StageId);
            builder.AppendLine($"  - node_id: {node.NodeId}");
            builder.AppendLine($"    name: {Value(node.Name)}");
            builder.AppendLine($"    stage_id: {(node.StageId?.ToString() ?? "null")}");
            builder.AppendLine($"    stage_name: {Value(stage?.Name)}");
            builder.AppendLine($"    stage_category: {(stage?.Category ?? "null")}");
            builder.AppendLine($"    command_id: {(node.CommandId?.ToString() ?? "null")}");
            builder.AppendLine($"    location_id: {(node.LocationId?.ToString() ?? "null")}");
            builder.AppendLine($"    manufacturer_vehicle_type_id: {(node.ManufacturerVehicleTypeId?.ToString() ?? "null")}");
            builder.AppendLine($"    rw_type: {node.RwType}");
        }
        return builder.ToString().TrimEnd();
    }

    private static Guid? ResolveStageIdByX(double x, IReadOnlyList<ExportStage> stages)
    {
        if (stages.Count == 0) return null;
        var index = Math.Max(0, Math.Min((int)Math.Round((x - NodeOffsetX) / StageWidth), stages.Count - 1));
        return stages[index].StageId;
    }
    private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
    private static string Value(string? value) => string.IsNullOrWhiteSpace(value) ? "null" : $"\"{Escape(value)}\"";
    private static string Safe(string value) => string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');

    private sealed record FlowSnapshot(int SchemaVersion, DateTime ExportedAtUtc, ExportProject Project, ExportFlow Flow, IReadOnlyList<ExportLane> Lanes, IReadOnlyList<ExportStage> Stages, IReadOnlyList<ExportNode> Nodes, IReadOnlyList<ExportLink> Links, IReadOnlyList<ExportComment> Comments, IReadOnlyList<ExportMetadata> Metadata);
    private sealed record ExportProject(Guid ProjectId, string Name);
    private sealed record ExportFlow(Guid FlowId, Guid ProjectId, string ProjectName, string Name, string? Description, int SortOrder, int CurrentRevision, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
    private sealed record ExportLane(Guid LaneId, Guid FlowId, string Name, int SortOrder);
    private sealed record ExportStage(Guid StageId, Guid FlowId, string Name, string Category, string StageType, int SortOrder);
    private sealed record ExportNode(Guid NodeId, Guid FlowId, Guid? LaneId, Guid? StageId, string NodeType, string Name, string? Description, double X, double Y, Guid? CommandId, Guid? LocationId, Guid? ManufacturerVehicleTypeId, string? RwType);
    private sealed record ExportLink(Guid LinkId, Guid FlowId, Guid SourceNodeId, Guid TargetNodeId, string? Label, string? Condition);
    private sealed record ExportComment(Guid CommentId, Guid FlowId, Guid? NodeId, string Text, double X, double Y);
    private sealed record ExportMetadata(Guid MetadataId, Guid FlowId, string MetaKey, string MetaValue);
}
