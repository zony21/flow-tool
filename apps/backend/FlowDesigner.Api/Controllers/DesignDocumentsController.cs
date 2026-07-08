using System.Text;
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
[Route("api/projects/{projectId:guid}/flows/{flowId:guid}/export/design-doc")]
[Authorize]
public sealed class DesignDocumentsController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportDesignDocument(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var flow = await dbContext.Flows
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.FlowId == flowId)
            .Select(x => new
            {
                x.FlowId,
                ProjectName = x.Project.Name,
                FlowName = x.Name,
                x.Description,
                x.Revision,
                x.UpdatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (flow is null)
        {
            return ApiError.NotFound<TextExportResponse>(this, "Flow was not found.");
        }

        var lanes = await dbContext.Lanes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.LaneId, x.Name, x.SortOrder })
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.StageId, x.Name, x.SortOrder })
            .ToListAsync(cancellationToken);

        var nodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.Y)
            .ThenBy(x => x.X)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.NodeId,
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
                x.SourceNodeId,
                x.TargetNodeId,
                x.Label,
                x.Condition,
            })
            .ToListAsync(cancellationToken);

        var comments = await dbContext.Comments.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.Y)
            .ThenBy(x => x.X)
            .Select(x => new { x.CommentId, x.NodeId, x.Text, x.X, x.Y })
            .ToListAsync(cancellationToken);

        var lanesById = lanes.ToDictionary(x => x.LaneId, x => x.Name);
        var stagesById = stages.ToDictionary(x => x.StageId, x => x.Name);
        var nodesById = nodes.ToDictionary(x => x.NodeId, x => x);
        var outgoing = links.GroupBy(x => x.SourceNodeId).ToDictionary(x => x.Key, x => x.ToList());
        var incoming = links.GroupBy(x => x.TargetNodeId).ToDictionary(x => x.Key, x => x.ToList());
        var nodeKeys = nodes.Select((node, index) => new { node.NodeId, Key = $"N{index + 1}" }).ToDictionary(x => x.NodeId, x => x.Key);

        var builder = new StringBuilder();
        builder.AppendLine($"# {EscapeMarkdown(flow.FlowName)} 設計書");
        builder.AppendLine();
        builder.AppendLine("## 1. フロー概要");
        builder.AppendLine();
        builder.AppendLine($"- プロジェクト: {EscapeMarkdown(flow.ProjectName)}");
        builder.AppendLine($"- フロー: {EscapeMarkdown(flow.FlowName)}");
        builder.AppendLine($"- Revision: {flow.Revision}");
        builder.AppendLine($"- 最終更新: {flow.UpdatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine($"- 説明: {EscapeMarkdown(string.IsNullOrWhiteSpace(flow.Description) ? "未設定" : flow.Description)}");
        builder.AppendLine();

        builder.AppendLine("## 2. 構造定義");
        builder.AppendLine();
        builder.AppendLine("### 2.1 工程分類（Lane）");
        builder.AppendLine();
        builder.AppendLine("|順序|工程分類|");
        builder.AppendLine("|---:|---|");
        foreach (var lane in lanes)
        {
            builder.AppendLine($"|{lane.SortOrder}|{EscapeMarkdown(lane.Name)}|");
        }
        builder.AppendLine();

        builder.AppendLine("### 2.2 設備・担当（Stage）");
        builder.AppendLine();
        builder.AppendLine("|順序|設備・担当|");
        builder.AppendLine("|---:|---|");
        foreach (var stage in stages)
        {
            builder.AppendLine($"|{stage.SortOrder}|{EscapeMarkdown(stage.Name)}|");
        }
        builder.AppendLine();

        builder.AppendLine("## 3. 工程別処理一覧");
        builder.AppendLine();
        foreach (var lane in lanes)
        {
            var laneNodes = nodes.Where(node => node.LaneId == lane.LaneId).ToList();
            if (laneNodes.Count == 0)
            {
                continue;
            }

            builder.AppendLine($"### 3.{lane.SortOrder} {EscapeMarkdown(lane.Name)}");
            builder.AppendLine();
            builder.AppendLine("|No|設備・担当|処理|種別|説明|次処理|");
            builder.AppendLine("|---|---|---|---|---|---|");

            foreach (var node in laneNodes.OrderBy(x => x.Y).ThenBy(x => x.X).ThenBy(x => x.Name))
            {
                var nextText = outgoing.TryGetValue(node.NodeId, out var nextLinks) && nextLinks.Count > 0
                    ? string.Join("<br>", nextLinks.Select(link => FormatNext(link.TargetNodeId, link.Label, link.Condition, nodesById, nodeKeys)))
                    : "なし";

                builder.AppendLine($"|{nodeKeys[node.NodeId]}|{EscapeMarkdown(GetStageName(node.StageId, stagesById))}|{EscapeMarkdown(node.Name)}|{EscapeMarkdown(node.NodeType)}|{EscapeMarkdown(node.Description ?? "")}|{nextText}|");
            }

            builder.AppendLine();
        }

        var unassignedNodes = nodes.Where(node => node.LaneId is null || !lanesById.ContainsKey(node.LaneId.Value)).ToList();
        if (unassignedNodes.Count > 0)
        {
            builder.AppendLine("### 3.x 未設定");
            builder.AppendLine();
            builder.AppendLine("|No|設備・担当|処理|種別|説明|次処理|");
            builder.AppendLine("|---|---|---|---|---|---|");
            foreach (var node in unassignedNodes)
            {
                builder.AppendLine($"|{nodeKeys[node.NodeId]}|{EscapeMarkdown(GetStageName(node.StageId, stagesById))}|{EscapeMarkdown(node.Name)}|{EscapeMarkdown(node.NodeType)}|{EscapeMarkdown(node.Description ?? "")}|なし|");
            }
            builder.AppendLine();
        }

        builder.AppendLine("## 4. ノード一覧");
        builder.AppendLine();
        builder.AppendLine("|No|工程分類|設備・担当|処理名|種別|位置|");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var node in nodes)
        {
            builder.AppendLine($"|{nodeKeys[node.NodeId]}|{EscapeMarkdown(GetLaneName(node.LaneId, lanesById))}|{EscapeMarkdown(GetStageName(node.StageId, stagesById))}|{EscapeMarkdown(node.Name)}|{EscapeMarkdown(node.NodeType)}|x={node.X}, y={node.Y}|");
        }
        builder.AppendLine();

        builder.AppendLine("## 5. 接続一覧");
        builder.AppendLine();
        builder.AppendLine("|From|To|条件・ラベル|");
        builder.AppendLine("|---|---|---|");
        if (links.Count == 0)
        {
            builder.AppendLine("|なし|なし|なし|");
        }
        else
        {
            foreach (var link in links)
            {
                var from = nodesById.TryGetValue(link.SourceNodeId, out var sourceNode) ? $"{nodeKeys[sourceNode.NodeId]} {sourceNode.Name}" : link.SourceNodeId.ToString();
                var to = nodesById.TryGetValue(link.TargetNodeId, out var targetNode) ? $"{nodeKeys[targetNode.NodeId]} {targetNode.Name}" : link.TargetNodeId.ToString();
                builder.AppendLine($"|{EscapeMarkdown(from)}|{EscapeMarkdown(to)}|{EscapeMarkdown(ChooseLinkLabel(link.Label, link.Condition))}|");
            }
        }
        builder.AppendLine();

        builder.AppendLine("## 6. 入出力関係");
        builder.AppendLine();
        foreach (var node in nodes)
        {
            builder.AppendLine($"### {nodeKeys[node.NodeId]} {EscapeMarkdown(node.Name)}");
            builder.AppendLine();
            builder.AppendLine($"- 工程分類: {EscapeMarkdown(GetLaneName(node.LaneId, lanesById))}");
            builder.AppendLine($"- 設備・担当: {EscapeMarkdown(GetStageName(node.StageId, stagesById))}");
            builder.AppendLine("- 前工程:");
            if (!incoming.TryGetValue(node.NodeId, out var previousLinks) || previousLinks.Count == 0)
            {
                builder.AppendLine("  - なし");
            }
            else
            {
                foreach (var link in previousLinks)
                {
                    builder.AppendLine($"  - {EscapeMarkdown(FormatPrevious(link.SourceNodeId, link.Label, link.Condition, nodesById, nodeKeys))}");
                }
            }
            builder.AppendLine("- 次工程:");
            if (!outgoing.TryGetValue(node.NodeId, out var nextLinks) || nextLinks.Count == 0)
            {
                builder.AppendLine("  - なし");
            }
            else
            {
                foreach (var link in nextLinks)
                {
                    builder.AppendLine($"  - {EscapeMarkdown(FormatNext(link.TargetNodeId, link.Label, link.Condition, nodesById, nodeKeys))}");
                }
            }
            builder.AppendLine();
        }

        if (comments.Count > 0)
        {
            builder.AppendLine("## 7. コメント");
            builder.AppendLine();
            builder.AppendLine("|対象|コメント|位置|");
            builder.AppendLine("|---|---|---|");
            foreach (var commentItem in comments)
            {
                var target = commentItem.NodeId is Guid nodeId && nodesById.TryGetValue(nodeId, out var node)
                    ? $"{nodeKeys[node.NodeId]} {node.Name}"
                    : "フロー全体";
                builder.AppendLine($"|{EscapeMarkdown(target)}|{EscapeMarkdown(commentItem.Text)}|x={commentItem.X}, y={commentItem.Y}|");
            }
            builder.AppendLine();
        }

        builder.AppendLine("## 8. AI解析用補足");
        builder.AppendLine();
        builder.AppendLine("この設計書では、工程分類を action、設備・担当を owner として扱う。");
        builder.AppendLine("各ノードは action / owner / next の組で工程復元できるように記載している。");

        return Ok(new TextExportResponse($"{BuildFileName(flow.ProjectName, flow.FlowName)}_design.md", builder.ToString().TrimEnd()));
    }

    private static string GetLaneName(Guid? laneId, IReadOnlyDictionary<Guid, string> lanesById)
    {
        return laneId is Guid id && lanesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string GetStageName(Guid? stageId, IReadOnlyDictionary<Guid, string> stagesById)
    {
        return stageId is Guid id && stagesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string ChooseLinkLabel(string? label, string? condition)
    {
        return !string.IsNullOrWhiteSpace(condition) ? condition : string.IsNullOrWhiteSpace(label) ? "" : label;
    }

    private static string FormatNext<TNode>(Guid targetNodeId, string? label, string? condition, IReadOnlyDictionary<Guid, TNode> nodesById, IReadOnlyDictionary<Guid, string> nodeKeys)
        where TNode : class
    {
        var nodeName = GetNodeName(targetNodeId, nodesById);
        var key = nodeKeys.GetValueOrDefault(targetNodeId, targetNodeId.ToString());
        var linkLabel = ChooseLinkLabel(label, condition);
        return string.IsNullOrWhiteSpace(linkLabel) ? $"{key} {nodeName}" : $"{key} {nodeName}（{linkLabel}）";
    }

    private static string FormatPrevious<TNode>(Guid sourceNodeId, string? label, string? condition, IReadOnlyDictionary<Guid, TNode> nodesById, IReadOnlyDictionary<Guid, string> nodeKeys)
        where TNode : class
    {
        var nodeName = GetNodeName(sourceNodeId, nodesById);
        var key = nodeKeys.GetValueOrDefault(sourceNodeId, sourceNodeId.ToString());
        var linkLabel = ChooseLinkLabel(label, condition);
        return string.IsNullOrWhiteSpace(linkLabel) ? $"{key} {nodeName}" : $"{key} {nodeName}（{linkLabel}）";
    }

    private static string GetNodeName<TNode>(Guid nodeId, IReadOnlyDictionary<Guid, TNode> nodesById)
        where TNode : class
    {
        if (!nodesById.TryGetValue(nodeId, out var node))
        {
            return nodeId.ToString();
        }

        var property = typeof(TNode).GetProperty("Name");
        return property?.GetValue(node)?.ToString() ?? nodeId.ToString();
    }

    private static string EscapeMarkdown(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal);
    }

    private static string BuildFileName(string projectName, string flowName)
    {
        var raw = $"{projectName}_{flowName}";
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(raw.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "flow" : safe;
    }
}
