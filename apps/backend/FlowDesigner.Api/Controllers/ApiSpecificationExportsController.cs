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
[Route("api/projects/{projectId:guid}/flows/{flowId:guid}/export/api-spec")]
[Authorize]
public sealed class ApiSpecificationExportsController(AppDbContext dbContext) : ControllerBase
{
    private const double StageWidth = 240;
    private const double NodeOffsetX = 42;

    [HttpPost]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportApiSpecification(Guid projectId, Guid flowId, CancellationToken cancellationToken)
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
            .Select(x => new ApiLane(x.LaneId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var stages = await dbContext.Stages.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .OrderBy(x => x.SortOrder)
            .Select(x => new ApiStage(x.StageId, x.Name, x.SortOrder))
            .ToListAsync(cancellationToken);

        var rawNodes = await dbContext.Nodes.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new ApiNode(x.NodeId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y))
            .ToListAsync(cancellationToken);

        var nodes = NormalizeNodes(rawNodes, lanes, stages)
            .OrderBy(x => x.Y)
            .ThenBy(x => x.X)
            .ThenBy(x => x.Name)
            .ToList();

        var links = await dbContext.Links.AsNoTracking()
            .Where(x => x.FlowId == flowId)
            .Select(x => new ApiLink(x.LinkId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition))
            .ToListAsync(cancellationToken);

        var lanesById = lanes.ToDictionary(x => x.LaneId, x => x.Name);
        var stagesById = stages.ToDictionary(x => x.StageId, x => x.Name);
        var nodesById = nodes.ToDictionary(x => x.NodeId, x => x);
        var nodeKeys = nodes.Select((node, index) => new { node.NodeId, Key = $"N{index + 1}" }).ToDictionary(x => x.NodeId, x => x.Key);

        var communicationLinks = links
            .Where(link => IsCommunicationLink(link, nodesById))
            .OrderBy(link => nodesById.TryGetValue(link.SourceNodeId, out var node) ? node.Y : double.MaxValue)
            .ThenBy(link => nodesById.TryGetValue(link.SourceNodeId, out var node) ? node.X : double.MaxValue)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"# {EscapeMarkdown(flow.FlowName)} API・通信仕様書");
        builder.AppendLine();
        builder.AppendLine("## 1. 概要");
        builder.AppendLine();
        builder.AppendLine($"- プロジェクト: {EscapeMarkdown(flow.ProjectName)}");
        builder.AppendLine($"- フロー: {EscapeMarkdown(flow.FlowName)}");
        builder.AppendLine($"- Revision: {flow.Revision}");
        builder.AppendLine($"- 最終更新: {flow.UpdatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
        builder.AppendLine("- 注意: 本書はフロー構造から自動生成したAPI/通信仕様の下書きです。実際のURL、HTTPメソッド、項目型は詳細設計で確定してください。");
        builder.AppendLine();

        builder.AppendLine("## 2. 通信候補一覧");
        builder.AppendLine();
        builder.AppendLine("|No|送信元|受信先|送信元処理|受信先処理|データ/条件|通信種別|");
        builder.AppendLine("|---|---|---|---|---|---|---|");
        if (communicationLinks.Count == 0)
        {
            builder.AppendLine("|なし|なし|なし|なし|なし|なし|なし|");
        }
        else
        {
            var index = 1;
            foreach (var link in communicationLinks)
            {
                if (!nodesById.TryGetValue(link.SourceNodeId, out var source) || !nodesById.TryGetValue(link.TargetNodeId, out var target))
                {
                    continue;
                }

                builder.AppendLine($"|API-{index:000}|{EscapeMarkdown(GetStageName(source.StageId, stagesById))}|{EscapeMarkdown(GetStageName(target.StageId, stagesById))}|{EscapeMarkdown($"{nodeKeys[source.NodeId]} {source.Name}")}|{EscapeMarkdown($"{nodeKeys[target.NodeId]} {target.Name}")}|{EscapeMarkdown(ChooseLabel(link))}|{EscapeMarkdown(ClassifyCommunication(link, source, target))}|");
                index++;
            }
        }
        builder.AppendLine();

        builder.AppendLine("## 3. 通信詳細");
        builder.AppendLine();
        if (communicationLinks.Count == 0)
        {
            builder.AppendLine("通信候補はありません。");
            builder.AppendLine();
        }
        else
        {
            var index = 1;
            foreach (var link in communicationLinks)
            {
                if (!nodesById.TryGetValue(link.SourceNodeId, out var source) || !nodesById.TryGetValue(link.TargetNodeId, out var target))
                {
                    continue;
                }

                var apiName = BuildApiName(source, target, link);
                builder.AppendLine($"### API-{index:000} {EscapeMarkdown(apiName)}");
                builder.AppendLine();
                builder.AppendLine($"- 送信元: {EscapeMarkdown(GetStageName(source.StageId, stagesById))}");
                builder.AppendLine($"- 受信先: {EscapeMarkdown(GetStageName(target.StageId, stagesById))}");
                builder.AppendLine($"- 工程分類: {EscapeMarkdown(GetLaneName(source.LaneId, lanesById))} → {EscapeMarkdown(GetLaneName(target.LaneId, lanesById))}");
                builder.AppendLine($"- 起点処理: {EscapeMarkdown(source.Name)}");
                builder.AppendLine($"- 到達処理: {EscapeMarkdown(target.Name)}");
                builder.AppendLine($"- データ/条件: {EscapeMarkdown(ChooseLabel(link) == string.Empty ? "未設定" : ChooseLabel(link))}");
                builder.AppendLine($"- 通信種別: {EscapeMarkdown(ClassifyCommunication(link, source, target))}");
                builder.AppendLine("- 想定IF名: `" + EscapeCode(apiName) + "`");
                builder.AppendLine("- 想定HTTPメソッド: 未定");
                builder.AppendLine("- 想定URL: 未定");
                builder.AppendLine("- 認証方式: 未定");
                builder.AppendLine("- リトライ: 未定");
                builder.AppendLine("- タイムアウト: 未定");
                builder.AppendLine();
                builder.AppendLine("#### 要求項目候補");
                builder.AppendLine();
                builder.AppendLine("|項目名|型|必須|説明|");
                builder.AppendLine("|---|---|---|---|");
                foreach (var field in InferFields(link, source, target))
                {
                    builder.AppendLine($"|{EscapeMarkdown(field)}|未定|未定|フロー上のラベル/処理名から抽出|");
                }
                builder.AppendLine();
                builder.AppendLine("#### 応答項目候補");
                builder.AppendLine();
                builder.AppendLine("|項目名|型|必須|説明|");
                builder.AppendLine("|---|---|---|---|");
                builder.AppendLine("|result|未定|未定|処理結果|");
                builder.AppendLine("|message|未定|未定|メッセージ|");
                builder.AppendLine();
                index++;
            }
        }

        builder.AppendLine("## 4. 未確定事項");
        builder.AppendLine();
        builder.AppendLine("|No|確認事項|理由|");
        builder.AppendLine("|---|---|---|");
        builder.AppendLine("|1|HTTP/PLC/FTP/DB等の実通信方式|フロー構造だけでは確定できないため|");
        builder.AppendLine("|2|URL・エンドポイント名|実装方針に依存するため|");
        builder.AppendLine("|3|要求/応答の型・桁数・必須区分|項目定義が別途必要なため|");
        builder.AppendLine("|4|異常時レスポンス・リトライ・タイムアウト|運用要件に依存するため|");
        builder.AppendLine();

        return Ok(new TextExportResponse($"{BuildFileName(flow.ProjectName, flow.FlowName)}_api_spec.md", builder.ToString().TrimEnd()));
    }

    private static IReadOnlyList<ApiNode> NormalizeNodes(IReadOnlyList<ApiNode> nodes, IReadOnlyList<ApiLane> lanes, IReadOnlyList<ApiStage> stages)
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

    private static Guid? ResolveStageIdByX(double x, IReadOnlyList<ApiStage> stages)
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

    private static bool IsCommunicationLink(ApiLink link, IReadOnlyDictionary<Guid, ApiNode> nodesById)
    {
        if (!nodesById.TryGetValue(link.SourceNodeId, out var source) || !nodesById.TryGetValue(link.TargetNodeId, out var target))
        {
            return false;
        }

        return source.StageId != target.StageId
            || ContainsAny(source.Name, "送信", "受信", "中継", "連携", "通知")
            || ContainsAny(target.Name, "送信", "受信", "中継", "連携", "通知")
            || !string.IsNullOrWhiteSpace(link.Label)
            || !string.IsNullOrWhiteSpace(link.Condition);
    }

    private static string ClassifyCommunication(ApiLink link, ApiNode source, ApiNode target)
    {
        if (source.StageId != target.StageId)
        {
            return "設備間/システム間通信";
        }

        if (!string.IsNullOrWhiteSpace(link.Condition))
        {
            return "条件付き通信";
        }

        if (!string.IsNullOrWhiteSpace(link.Label))
        {
            return "データ受け渡し";
        }

        return "処理遷移";
    }

    private static string BuildApiName(ApiNode source, ApiNode target, ApiLink link)
    {
        var label = ChooseLabel(link);
        if (!string.IsNullOrWhiteSpace(label))
        {
            return $"{label} 連携";
        }

        return $"{source.Name} to {target.Name}";
    }

    private static IEnumerable<string> InferFields(ApiLink link, ApiNode source, ApiNode target)
    {
        var values = new List<string>();
        AddFieldCandidates(values, ChooseLabel(link));
        AddFieldCandidates(values, source.Name);
        AddFieldCandidates(values, target.Name);
        AddFieldCandidates(values, source.Description);
        AddFieldCandidates(values, target.Description);

        return values.Count == 0 ? ["未定"] : values.Distinct().Take(12);
    }

    private static void AddFieldCandidates(List<string> values, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var normalized = text.Replace("、", ",", StringComparison.Ordinal)
            .Replace("，", ",", StringComparison.Ordinal)
            .Replace("・", ",", StringComparison.Ordinal)
            .Replace("/", ",", StringComparison.Ordinal)
            .Replace(" ", ",", StringComparison.Ordinal);

        foreach (var item in normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (item.Length is >= 2 and <= 30 && !ContainsAny(item, "送信", "受信", "中継", "処理", "成功", "開始", "完了", "表示"))
            {
                values.Add(item);
            }
        }
    }

    private static string GetLaneName(Guid? laneId, IReadOnlyDictionary<Guid, string> lanesById)
    {
        return laneId is Guid id && lanesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string GetStageName(Guid? stageId, IReadOnlyDictionary<Guid, string> stagesById)
    {
        return stageId is Guid id && stagesById.TryGetValue(id, out var name) ? name : "未設定";
    }

    private static string ChooseLabel(ApiLink link)
    {
        return !string.IsNullOrWhiteSpace(link.Condition) ? link.Condition : string.IsNullOrWhiteSpace(link.Label) ? string.Empty : link.Label;
    }

    private static bool ContainsAny(string? text, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static string EscapeMarkdown(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal);
    }

    private static string EscapeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace("`", string.Empty, StringComparison.Ordinal);
    }

    private static string BuildFileName(string projectName, string flowName)
    {
        var raw = $"{projectName}_{flowName}";
        var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(raw.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "flow" : safe;
    }

    private sealed record ApiLane(Guid LaneId, string Name, int SortOrder);
    private sealed record ApiStage(Guid StageId, string Name, int SortOrder);
    private sealed record ApiNode(Guid NodeId, Guid? LaneId, Guid? StageId, string NodeType, string Name, string? Description, double X, double Y);
    private sealed record ApiLink(Guid LinkId, Guid SourceNodeId, Guid TargetNodeId, string? Label, string? Condition);
}
