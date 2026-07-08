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
[Route("api/projects/{projectId:guid}/flows/{flowId:guid}/export/ai-dsl-v2")]
[Authorize]
public sealed class AiDslV2ExportsController(AppDbContext dbContext) : ControllerBase
{
    private const double StageWidth = 240;
    private const double NodeOffsetX = 42;

    [HttpPost]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportAiDslV2(Guid projectId, Guid flowId, CancellationToken cancellationToken)
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
            .Select(x => new AiLane(x.LaneId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new AiStage(x.StageId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var rawNodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new AiNode(x.NodeId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var nodes = NormalizeNodes(rawNodes, lanes, stages);
        var laneSort = lanes.ToDictionary(x => x.LaneId, x => x.SortOrder);
        var stageSort = stages.ToDictionary(x => x.StageId, x => x.SortOrder);
        var orderedNodes = nodes
            .OrderBy(x => x.LaneId is Guid laneId && laneSort.TryGetValue(laneId, out var laneOrder) ? laneOrder : int.MaxValue)
            .ThenBy(x => x.Y)
            .ThenBy(x => x.StageId is Guid stageId && stageSort.TryGetValue(stageId, out var stageOrder) ? stageOrder : int.MaxValue)
            .ThenBy(x => x.X)
            .ThenBy(x => x.Name)
            .ToList();

        var links = await dbContext.Links.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new AiLink(x.LinkId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition))
            .ToListAsync(cancellationToken);

        var comments = await dbContext.Comments.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new AiComment(x.CommentId, x.NodeId, x.Text, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var lanesById = lanes.ToDictionary(x => x.LaneId, x => x.Name);
        var stagesById = stages.ToDictionary(x => x.StageId, x => x.Name);
        var nodesById = orderedNodes.ToDictionary(x => x.NodeId, x => x);
        var nodeKeys = orderedNodes.Select((node, index) => new { node.NodeId, Key = $"N{index + 1}" }).ToDictionary(x => x.NodeId, x => x.Key);
        var outgoing = links.GroupBy(x => x.SourceNodeId).ToDictionary(x => x.Key, x => x.ToList());
        var incoming = links.GroupBy(x => x.TargetNodeId).ToDictionary(x => x.Key, x => x.ToList());

        var builder = new StringBuilder();
        builder.AppendLine("AI_FLOW_DSL_VERSION: 2");
        builder.AppendLine("PURPOSE: AI reconstruction of process flow with explicit responsibility, branches, exceptions, loops, and inter-system communication.");
        builder.AppendLine();
        builder.AppendLine("FLOW:");
        builder.AppendLine($"  project_name: {DslValue(flow.ProjectName)}");
        builder.AppendLine($"  flow_name: {DslValue(flow.FlowName)}");
        builder.AppendLine($"  flow_description: {DslValue(flow.Description)}");
        builder.AppendLine($"  revision: {flow.Revision}");
        builder.AppendLine($"  updated_at_utc: {flow.UpdatedAtUtc:O}");
        builder.AppendLine();

        builder.AppendLine("RESPONSIBILITY_DEFINITION:");
        builder.AppendLine("  lane_means: action_or_process_category");
        builder.AppendLine("  stage_means: owner_equipment_system_or_department");
        builder.AppendLine("  node_means: concrete_processing_step");
        builder.AppendLine("  link_means: actual_transition_or_message_flow");
        builder.AppendLine();

        builder.AppendLine("RESPONSIBILITY_MATRIX:");
        foreach (var lane in lanes)
        {
            builder.AppendLine($"  - action: {DslValue(lane.Name)}");
            builder.AppendLine("    owners:");
            var ownerCount = 0;
            foreach (var stage in stages)
            {
                var ownerNodes = orderedNodes.Where(node => node.LaneId == lane.LaneId && node.StageId == stage.StageId).ToList();
                if (ownerNodes.Count == 0)
                {
                    continue;
                }

                ownerCount++;
                builder.AppendLine($"      - owner: {DslValue(stage.Name)}");
                builder.AppendLine("        nodes:");
                foreach (var node in ownerNodes)
                {
                    builder.AppendLine($"          - {nodeKeys[node.NodeId]}: {DslValue(node.Name)}");
                }
            }

            if (ownerCount == 0)
            {
                builder.AppendLine("      - none");
            }
        }
        builder.AppendLine();

        builder.AppendLine("NODES:");
        foreach (var node in orderedNodes)
        {
            var previousLinks = incoming.TryGetValue(node.NodeId, out var previous) ? previous : [];
            var nextLinks = outgoing.TryGetValue(node.NodeId, out var next) ? next : [];
            var nodeText = $"{node.Name} {node.Description}";

            builder.AppendLine($"  - key: {nodeKeys[node.NodeId]}");
            builder.AppendLine($"    node_id: {node.NodeId}");
            builder.AppendLine($"    name: {DslValue(node.Name)}");
            builder.AppendLine($"    type: {DslValue(node.NodeType)}");
            builder.AppendLine($"    action: {DslValue(GetLaneName(node.LaneId, lanesById))}");
            builder.AppendLine($"    owner: {DslValue(GetStageName(node.StageId, stagesById))}");
            builder.AppendLine($"    responsibility: action={DslValue(GetLaneName(node.LaneId, lanesById))}, owner={DslValue(GetStageName(node.StageId, stagesById))}");
            builder.AppendLine($"    description: {DslValue(node.Description)}");
            builder.AppendLine($"    position: x={node.X}, y={node.Y}");
            builder.AppendLine($"    classification: {ClassifyNode(nodeText)}");
            builder.AppendLine("    previous:");
            if (previousLinks.Count == 0)
            {
                builder.AppendLine("      - none");
            }
            else
            {
                foreach (var link in previousLinks)
                {
                    builder.AppendLine($"      - from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                    builder.AppendLine($"        label: {DslValue(ChooseLinkLabel(link))}");
                }
            }
            builder.AppendLine("    next:");
            if (nextLinks.Count == 0)
            {
                builder.AppendLine("      - none");
            }
            else
            {
                foreach (var link in nextLinks)
                {
                    builder.AppendLine($"      - to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                    builder.AppendLine($"        label: {DslValue(ChooseLinkLabel(link))}");
                    builder.AppendLine($"        transition_type: {ClassifyTransition(link, nodesById)}");
                }
            }
        }
        builder.AppendLine();

        builder.AppendLine("PROCESS_FLOW_LINKS:");
        if (links.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var link in links)
            {
                nodesById.TryGetValue(link.SourceNodeId, out var sourceNode);
                nodesById.TryGetValue(link.TargetNodeId, out var targetNode);
                builder.AppendLine($"  - from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                builder.AppendLine($"    to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                builder.AppendLine($"    label: {DslValue(ChooseLinkLabel(link))}");
                builder.AppendLine($"    source_owner: {DslValue(sourceNode is null ? null : GetStageName(sourceNode.StageId, stagesById))}");
                builder.AppendLine($"    target_owner: {DslValue(targetNode is null ? null : GetStageName(targetNode.StageId, stagesById))}");
                builder.AppendLine($"    transition_type: {ClassifyTransition(link, nodesById)}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("CONDITIONS_AND_BRANCHES:");
        var conditionalLinks = links.Where(link => !string.IsNullOrWhiteSpace(link.Condition) || !string.IsNullOrWhiteSpace(link.Label)).ToList();
        if (conditionalLinks.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var link in conditionalLinks)
            {
                builder.AppendLine($"  - from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                builder.AppendLine($"    to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                builder.AppendLine($"    condition_or_message: {DslValue(ChooseLinkLabel(link))}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("INTER_OWNER_COMMUNICATIONS:");
        var communicationLinks = links.Where(link => IsInterOwnerCommunication(link, nodesById)).ToList();
        if (communicationLinks.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var link in communicationLinks)
            {
                var sourceNode = nodesById[link.SourceNodeId];
                var targetNode = nodesById[link.TargetNodeId];
                builder.AppendLine($"  - from_owner: {DslValue(GetStageName(sourceNode.StageId, stagesById))}");
                builder.AppendLine($"    from_node: {nodeKeys[sourceNode.NodeId]}");
                builder.AppendLine($"    to_owner: {DslValue(GetStageName(targetNode.StageId, stagesById))}");
                builder.AppendLine($"    to_node: {nodeKeys[targetNode.NodeId]}");
                builder.AppendLine($"    message_or_condition: {DslValue(ChooseLinkLabel(link))}");
            }
        }
        builder.AppendLine();

        builder.AppendLine("LOOP_CANDIDATES:");
        var loopLinks = links.Where(link => IsLoopCandidate(link, nodeKeys)).ToList();
        if (loopLinks.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var link in loopLinks)
            {
                builder.AppendLine($"  - from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                builder.AppendLine($"    to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                builder.AppendLine($"    reason: target node appears earlier than source node in visual/process order");
            }
        }
        builder.AppendLine();

        builder.AppendLine("EXCEPTION_CANDIDATES:");
        var exceptionNodes = orderedNodes.Where(node => LooksExceptional(node.Name) || LooksExceptional(node.Description)).ToList();
        var exceptionLinks = links.Where(link => LooksExceptional(link.Label) || LooksExceptional(link.Condition)).ToList();
        if (exceptionNodes.Count == 0 && exceptionLinks.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var node in exceptionNodes)
            {
                builder.AppendLine($"  - node: {nodeKeys[node.NodeId]}");
                builder.AppendLine($"    reason: node text suggests exception or abnormal handling");
            }
            foreach (var link in exceptionLinks)
            {
                builder.AppendLine($"  - link_from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                builder.AppendLine($"    link_to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                builder.AppendLine($"    reason: link label or condition suggests exception path");
            }
        }

        if (comments.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("COMMENTS:");
            foreach (var comment in comments)
            {
                builder.AppendLine($"  - node: {(comment.NodeId is Guid nodeId ? nodeKeys.GetValueOrDefault(nodeId, nodeId.ToString()) : "none")}");
                builder.AppendLine($"    text: {DslValue(comment.Text)}");
                builder.AppendLine($"    position: x={comment.X}, y={comment.Y}");
            }
        }

        return Ok(new TextExportResponse($"{BuildFileName(flow.ProjectName, flow.FlowName)}_ai_dsl_v2.flowdsl.txt", builder.ToString().TrimEnd()));
    }

    private static IReadOnlyList<AiNode> NormalizeNodes(IReadOnlyList<AiNode> nodes, IReadOnlyList<AiLane> lanes, IReadOnlyList<AiStage> stages)
    {
        var laneIds = lanes.Select(x => x.LaneId).ToHashSet();
        var stageIds = stages.Select(x => x.StageId).ToHashSet();
        return nodes.Select(node =>
        {
            var laneId = node.LaneId.HasValue && laneIds.Contains(node.LaneId.Value) ? node.LaneId : null;
            var stageId = node.StageId.HasValue && stageIds.Contains(node.StageId.Value) ? node.StageId : ResolveStageIdByX(node.X, stages);
            return node with { LaneId = laneId, StageId = stageId };
        }).ToList();
    }

    private static Guid? ResolveStageIdByX(double x, IReadOnlyList<AiStage> stages)
    {
        if (stages.Count == 0)
        {
            return null;
        }

        var sortedStages = stages.OrderBy(x => x.SortOrder).ToList();
        var index = (int)Math.Round((x - NodeOffsetX) / StageWidth);
        index = Math.Max(0, Math.Min(index, sortedStages.Count - 1));
        return sortedStages[index].StageId;
    }

    private static bool IsInterOwnerCommunication(AiLink link, IReadOnlyDictionary<Guid, AiNode> nodesById)
    {
        return nodesById.TryGetValue(link.SourceNodeId, out var source)
            && nodesById.TryGetValue(link.TargetNodeId, out var target)
            && source.StageId.HasValue
            && target.StageId.HasValue
            && source.StageId != target.StageId;
    }

    private static bool IsLoopCandidate(AiLink link, IReadOnlyDictionary<Guid, string> nodeKeys)
    {
        if (!nodeKeys.TryGetValue(link.SourceNodeId, out var sourceKey) || !nodeKeys.TryGetValue(link.TargetNodeId, out var targetKey))
        {
            return false;
        }

        return ParseNodeKeyNumber(targetKey) <= ParseNodeKeyNumber(sourceKey);
    }

    private static int ParseNodeKeyNumber(string nodeKey)
    {
        return int.TryParse(nodeKey.TrimStart('N'), out var number) ? number : int.MaxValue;
    }

    private static string ClassifyTransition(AiLink link, IReadOnlyDictionary<Guid, AiNode> nodesById)
    {
        if (LooksExceptional(link.Label) || LooksExceptional(link.Condition))
        {
            return "exception_or_abnormal_path";
        }

        if (IsInterOwnerCommunication(link, nodesById))
        {
            return "inter_owner_communication";
        }

        if (!string.IsNullOrWhiteSpace(link.Condition))
        {
            return "conditional_branch";
        }

        if (!string.IsNullOrWhiteSpace(link.Label))
        {
            return "message_or_labeled_transition";
        }

        return "normal_transition";
    }

    private static string ClassifyNode(string? text)
    {
        if (LooksExceptional(text))
        {
            return "exception_or_abnormal_step";
        }

        if (ContainsAny(text, "判定", "確認", "成功", "失敗", "あり", "なし", "OK", "NG"))
        {
            return "decision_or_check_step";
        }

        if (ContainsAny(text, "送信", "受信", "中継", "連携", "通知"))
        {
            return "communication_step";
        }

        return "normal_step";
    }

    private static bool LooksExceptional(string? text)
    {
        return ContainsAny(text, "異常", "例外", "エラー", "失敗", "NG", "キャンセル", "中断", "停止", "タイムアウト", "不可", "警告");
    }

    private static bool ContainsAny(string? text, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetLaneName(Guid? laneId, IReadOnlyDictionary<Guid, string> lanesById)
    {
        return laneId is Guid id && lanesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string GetStageName(Guid? stageId, IReadOnlyDictionary<Guid, string> stagesById)
    {
        return stageId is Guid id && stagesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string? ChooseLinkLabel(AiLink link)
    {
        return !string.IsNullOrWhiteSpace(link.Condition) ? link.Condition : link.Label;
    }

    private static string DslValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "null";
        }

        return $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal)}\"";
    }

    private static string BuildFileName(string projectName, string flowName)
    {
        var raw = $"{projectName}_{flowName}";
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(raw.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "flow" : safe;
    }

    private sealed record AiLane(Guid LaneId, string Name, int SortOrder);
    private sealed record AiStage(Guid StageId, string Name, int SortOrder);
    private sealed record AiNode(Guid NodeId, Guid? LaneId, Guid? StageId, string NodeType, string Name, string? Description, double X, double Y);
    private sealed record AiLink(Guid LinkId, Guid SourceNodeId, Guid TargetNodeId, string? Label, string? Condition);
    private sealed record AiComment(Guid CommentId, Guid? NodeId, string Text, double X, double Y);
}
