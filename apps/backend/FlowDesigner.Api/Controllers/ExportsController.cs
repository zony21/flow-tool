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

        var json = JsonSerializer.Serialize(BuildJsonExport(snapshot), ExportJsonOptions);
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

    [HttpPost("ai-dsl")]
    [RequirePermission(PermissionCodes.ExportExecute)]
    public async Task<ActionResult<TextExportResponse>> ExportAiDsl(Guid projectId, Guid flowId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(projectId, flowId, cancellationToken);
        if (snapshot is null)
        {
            return ApiError.NotFound<TextExportResponse>(this, "Flow was not found.");
        }

        var content = BuildAiDsl(snapshot);
        return Ok(new TextExportResponse(
            $"{BuildFileName(snapshot.ProjectName, snapshot.FlowName, "ai_dsl")}.flowdsl.txt",
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

        var laneSort = lanes.ToDictionary(x => x.LaneId, x => x.SortOrder);
        var stageSort = stages.ToDictionary(x => x.StageId, x => x.SortOrder);

        var exportNodes = nodes
            .Select(x => new ExportNode(x.NodeId, x.FlowId, x.LaneId, x.StageId, x.NodeType, x.Name, x.Description, x.X, x.Y))
            .OrderBy(x => x.LaneId is Guid laneId && laneSort.TryGetValue(laneId, out var laneOrder) ? laneOrder : int.MaxValue)
            .ThenBy(x => x.Y)
            .ThenBy(x => x.StageId is Guid stageId && stageSort.TryGetValue(stageId, out var stageOrder) ? stageOrder : int.MaxValue)
            .ThenBy(x => x.X)
            .ThenBy(x => x.Name)
            .ToList();

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
            exportNodes,
            links.Select(x => new ExportLink(x.LinkId, x.FlowId, x.SourceNodeId, x.TargetNodeId, x.Label, x.Condition)).ToList(),
            comments.Select(x => new ExportComment(x.CommentId, x.FlowId, x.NodeId, x.Text, x.X, x.Y)).ToList(),
            metadata.Select(x => new ExportMetadata(x.MetadataId, x.FlowId, x.MetaKey, x.MetaValue)).ToList());
    }

    private static object BuildJsonExport(FlowSnapshot snapshot)
    {
        var nodeKeys = BuildNodeKeys(snapshot.Nodes);
        var nodesById = snapshot.Nodes.ToDictionary(x => x.NodeId, x => x);
        var lanesById = snapshot.Lanes.ToDictionary(x => x.LaneId, x => x);
        var stagesById = snapshot.Stages.ToDictionary(x => x.StageId, x => x);
        var outgoing = snapshot.Links.GroupBy(x => x.SourceNodeId).ToDictionary(x => x.Key, x => x.ToList());
        var incoming = snapshot.Links.GroupBy(x => x.TargetNodeId).ToDictionary(x => x.Key, x => x.ToList());

        var enrichedNodes = snapshot.Nodes.Select((node, index) => new
        {
            key = nodeKeys[node.NodeId],
            visualOrder = index + 1,
            node.NodeId,
            node.FlowId,
            node.LaneId,
            laneName = GetLaneName(node, lanesById),
            node.StageId,
            stageName = GetStageName(node, stagesById),
            responsibility = new
            {
                action = GetLaneName(node, lanesById),
                owner = GetStageName(node, stagesById),
            },
            node.NodeType,
            node.Name,
            node.Description,
            position = new { node.X, node.Y },
            previous = incoming.TryGetValue(node.NodeId, out var previousLinks)
                ? previousLinks.Select(link => new
                {
                    link.LinkId,
                    fromKey = nodeKeys.GetValueOrDefault(link.SourceNodeId),
                    fromNodeId = link.SourceNodeId,
                    fromName = nodesById.TryGetValue(link.SourceNodeId, out var sourceNode) ? sourceNode.Name : null,
                    label = ChooseLinkLabel(link),
                }).ToList()
                : [],
            next = outgoing.TryGetValue(node.NodeId, out var nextLinks)
                ? nextLinks.Select(link => new
                {
                    link.LinkId,
                    toKey = nodeKeys.GetValueOrDefault(link.TargetNodeId),
                    toNodeId = link.TargetNodeId,
                    toName = nodesById.TryGetValue(link.TargetNodeId, out var targetNode) ? targetNode.Name : null,
                    label = ChooseLinkLabel(link),
                }).ToList()
                : [],
        }).ToList();

        var enrichedLinks = snapshot.Links.Select(link =>
        {
            nodesById.TryGetValue(link.SourceNodeId, out var sourceNode);
            nodesById.TryGetValue(link.TargetNodeId, out var targetNode);

            return new
            {
                link.LinkId,
                link.FlowId,
                link.SourceNodeId,
                sourceKey = nodeKeys.GetValueOrDefault(link.SourceNodeId),
                sourceName = sourceNode?.Name,
                sourceAction = sourceNode is null ? null : GetLaneName(sourceNode, lanesById),
                sourceOwner = sourceNode is null ? null : GetStageName(sourceNode, stagesById),
                link.TargetNodeId,
                targetKey = nodeKeys.GetValueOrDefault(link.TargetNodeId),
                targetName = targetNode?.Name,
                targetAction = targetNode is null ? null : GetLaneName(targetNode, lanesById),
                targetOwner = targetNode is null ? null : GetStageName(targetNode, stagesById),
                link.Label,
                link.Condition,
                effectiveLabel = ChooseLinkLabel(link),
            };
        }).ToList();

        var responsibilityMatrix = snapshot.Lanes.OrderBy(x => x.SortOrder).Select(lane => new
        {
            actionLaneId = lane.LaneId,
            action = lane.Name,
            owners = snapshot.Stages.OrderBy(x => x.SortOrder).Select(stage => new
            {
                ownerStageId = stage.StageId,
                owner = stage.Name,
                nodes = snapshot.Nodes
                    .Where(node => node.LaneId == lane.LaneId && node.StageId == stage.StageId)
                    .OrderBy(node => node.Y)
                    .ThenBy(node => node.X)
                    .ThenBy(node => node.Name)
                    .Select(node => new
                    {
                        key = nodeKeys[node.NodeId],
                        node.NodeId,
                        node.Name,
                        node.NodeType,
                        node.Description,
                        position = new { node.X, node.Y },
                        next = outgoing.TryGetValue(node.NodeId, out var nextLinks)
                            ? nextLinks.Select(link => new
                            {
                                toKey = nodeKeys.GetValueOrDefault(link.TargetNodeId),
                                toNodeId = link.TargetNodeId,
                                toName = nodesById.TryGetValue(link.TargetNodeId, out var targetNode) ? targetNode.Name : null,
                                label = ChooseLinkLabel(link),
                            }).ToList()
                            : [],
                    })
                    .ToList(),
            })
            .Where(ownerGroup => ownerGroup.nodes.Count > 0)
            .ToList(),
        }).ToList();

        var visualOrderByLane = snapshot.Lanes.OrderBy(x => x.SortOrder).Select(lane => new
        {
            lane.LaneId,
            lane.Name,
            nodes = snapshot.Nodes
                .Where(node => node.LaneId == lane.LaneId)
                .OrderBy(node => node.Y)
                .ThenBy(node => node.X)
                .ThenBy(node => node.Name)
                .Select(node => new
                {
                    key = nodeKeys[node.NodeId],
                    node.NodeId,
                    node.Name,
                    node.StageId,
                    stageName = GetStageName(node, stagesById),
                    position = new { node.X, node.Y },
                })
                .ToList(),
        }).ToList();

        return new
        {
            snapshot.SchemaVersion,
            snapshot.ExportedAtUtc,
            snapshot.Project,
            snapshot.Flow,
            snapshot.Lanes,
            snapshot.Stages,
            raw = new
            {
                snapshot.Nodes,
                snapshot.Links,
                snapshot.Comments,
                snapshot.Metadata,
            },
            flowGraph = new
            {
                nodes = enrichedNodes,
                links = enrichedLinks,
                visualOrderByLane,
                responsibilityMatrix,
            },
        };
    }

    private static string BuildMermaidFlowchart(FlowSnapshot snapshot, string direction, bool includeComments)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"flowchart {direction}");
        builder.AppendLine($"%% Flow: {EscapeMermaidComment(snapshot.Flow.Name)}");
        builder.AppendLine("%% Structure: 動作(Lane) -> 設備/担当(Stage) -> Node. Links below represent the actual process flow.");

        var nodeIds = BuildNodeKeys(snapshot.Nodes);
        var renderedNodeIds = new HashSet<Guid>();
        var laneIndex = 1;

        foreach (var lane in snapshot.Lanes.OrderBy(x => x.SortOrder))
        {
            var laneNodes = snapshot.Nodes.Where(node => node.LaneId == lane.LaneId).ToList();
            if (laneNodes.Count == 0)
            {
                continue;
            }

            builder.AppendLine($"    subgraph L{laneIndex}[\"動作: {EscapeMermaidLabel(lane.Name)}\"]");
            builder.AppendLine("        direction TD");

            var stageIndex = 1;
            foreach (var stage in snapshot.Stages.OrderBy(x => x.SortOrder))
            {
                var ownerNodes = laneNodes
                    .Where(node => node.StageId == stage.StageId)
                    .OrderBy(node => node.Y)
                    .ThenBy(node => node.X)
                    .ThenBy(node => node.Name)
                    .ToList();

                if (ownerNodes.Count == 0)
                {
                    continue;
                }

                builder.AppendLine($"        subgraph L{laneIndex}S{stageIndex}[\"設備/担当: {EscapeMermaidLabel(stage.Name)}\"]");
                builder.AppendLine("            direction TD");
                foreach (var node in ownerNodes)
                {
                    builder.AppendLine($"            %% responsibility: action={EscapeMermaidComment(lane.Name)} owner={EscapeMermaidComment(stage.Name)} x={node.X} y={node.Y}");
                    builder.AppendLine($"            {BuildMermaidNodeDeclaration(nodeIds[node.NodeId], node)}");
                    renderedNodeIds.Add(node.NodeId);
                }
                builder.AppendLine("        end");
                stageIndex++;
            }

            var laneUnassignedNodes = laneNodes
                .Where(node => node.StageId is null || snapshot.Stages.All(stage => stage.StageId != node.StageId))
                .OrderBy(node => node.Y)
                .ThenBy(node => node.X)
                .ThenBy(node => node.Name)
                .ToList();
            if (laneUnassignedNodes.Count > 0)
            {
                builder.AppendLine($"        subgraph L{laneIndex}SU[\"設備/担当: 未設定\"]");
                builder.AppendLine("            direction TD");
                foreach (var node in laneUnassignedNodes)
                {
                    builder.AppendLine($"            %% responsibility: action={EscapeMermaidComment(lane.Name)} owner=未設定 x={node.X} y={node.Y}");
                    builder.AppendLine($"            {BuildMermaidNodeDeclaration(nodeIds[node.NodeId], node)}");
                    renderedNodeIds.Add(node.NodeId);
                }
                builder.AppendLine("        end");
            }

            builder.AppendLine("    end");
            laneIndex++;
        }

        var unassignedNodes = snapshot.Nodes.Where(node => !renderedNodeIds.Contains(node.NodeId)).ToList();
        if (unassignedNodes.Count > 0)
        {
            builder.AppendLine("    subgraph UNASSIGNED[\"動作: 未設定\"]");
            builder.AppendLine("        direction TD");
            foreach (var node in unassignedNodes.OrderBy(node => node.Y).ThenBy(node => node.X).ThenBy(node => node.Name))
            {
                builder.AppendLine($"        %% responsibility: action=未設定 owner=未設定 x={node.X} y={node.Y}");
                builder.AppendLine($"        {BuildMermaidNodeDeclaration(nodeIds[node.NodeId], node)}");
            }
            builder.AppendLine("    end");
        }

        if (snapshot.Links.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("    %% Links: actual process flow");
        }

        foreach (var link in snapshot.Links)
        {
            if (!nodeIds.TryGetValue(link.SourceNodeId, out var source) || !nodeIds.TryGetValue(link.TargetNodeId, out var target))
            {
                continue;
            }

            var labelSource = ChooseLinkLabel(link);
            var label = string.IsNullOrWhiteSpace(labelSource) ? null : EscapeMermaidLabel(labelSource);
            builder.AppendLine(label is null
                ? $"    {source} --> {target}"
                : $"    {source} -->|\"{label}\"| {target}");
        }

        foreach (var node in snapshot.Nodes.Where(x => string.Equals(x.NodeType, "wait", StringComparison.OrdinalIgnoreCase)))
        {
            builder.AppendLine($"    style {nodeIds[node.NodeId]} stroke-dasharray: 6 4");
        }

        if (includeComments && snapshot.Comments.Count > 0)
        {
            builder.AppendLine();
            foreach (var comment in snapshot.Comments)
            {
                builder.AppendLine($"    %% Comment: {EscapeMermaidComment(comment.Text)}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildAiDsl(FlowSnapshot snapshot)
    {
        var builder = new StringBuilder();
        var nodeKeys = BuildNodeKeys(snapshot.Nodes);
        var lanesById = snapshot.Lanes.ToDictionary(x => x.LaneId, x => x);
        var stagesById = snapshot.Stages.ToDictionary(x => x.StageId, x => x);
        var outgoing = snapshot.Links.GroupBy(x => x.SourceNodeId).ToDictionary(x => x.Key, x => x.ToList());
        var incoming = snapshot.Links.GroupBy(x => x.TargetNodeId).ToDictionary(x => x.Key, x => x.ToList());

        builder.AppendLine("AI_FLOW_DSL_VERSION: 1");
        builder.AppendLine("PURPOSE: This file is optimized for AI understanding. Nodes, links, lanes, stages, and visual order are all explicit.");
        builder.AppendLine();
        builder.AppendLine("FLOW:");
        builder.AppendLine($"  project_name: {DslValue(snapshot.Project.Name)}");
        builder.AppendLine($"  flow_name: {DslValue(snapshot.Flow.Name)}");
        builder.AppendLine($"  flow_description: {DslValue(snapshot.Flow.Description)}");
        builder.AppendLine($"  revision: {snapshot.Flow.CurrentRevision}");
        builder.AppendLine($"  exported_at_utc: {snapshot.ExportedAtUtc:O}");
        builder.AppendLine();

        builder.AppendLine("RESPONSIBILITY_MATRIX:");
        foreach (var lane in snapshot.Lanes.OrderBy(x => x.SortOrder))
        {
            builder.AppendLine($"  - action: {DslValue(lane.Name)}");
            builder.AppendLine("    owners:");
            var hasOwner = false;
            foreach (var stage in snapshot.Stages.OrderBy(x => x.SortOrder))
            {
                var ownerNodes = snapshot.Nodes
                    .Where(node => node.LaneId == lane.LaneId && node.StageId == stage.StageId)
                    .OrderBy(node => node.Y)
                    .ThenBy(node => node.X)
                    .ThenBy(node => node.Name)
                    .ToList();
                if (ownerNodes.Count == 0)
                {
                    continue;
                }

                hasOwner = true;
                builder.AppendLine($"      - owner: {DslValue(stage.Name)}");
                builder.AppendLine("        nodes:");
                foreach (var node in ownerNodes)
                {
                    builder.AppendLine($"          - {nodeKeys[node.NodeId]}: {DslValue(node.Name)}");
                }
            }

            if (!hasOwner)
            {
                builder.AppendLine("      - none");
            }
        }
        builder.AppendLine();

        builder.AppendLine("LANES_PROCESS_CATEGORIES:");
        foreach (var lane in snapshot.Lanes.OrderBy(x => x.SortOrder))
        {
            builder.AppendLine($"  - lane_id: {lane.LaneId}");
            builder.AppendLine($"    name: {DslValue(lane.Name)}");
            builder.AppendLine($"    sort_order: {lane.SortOrder}");
        }
        builder.AppendLine();

        builder.AppendLine("STAGES_COLUMNS_OWNERS:");
        foreach (var stage in snapshot.Stages.OrderBy(x => x.SortOrder))
        {
            builder.AppendLine($"  - stage_id: {stage.StageId}");
            builder.AppendLine($"    name: {DslValue(stage.Name)}");
            builder.AppendLine($"    sort_order: {stage.SortOrder}");
        }
        builder.AppendLine();

        builder.AppendLine("NODES:");
        foreach (var node in snapshot.Nodes)
        {
            var laneName = GetLaneName(node, lanesById);
            var stageName = GetStageName(node, stagesById);
            var nextLinks = outgoing.TryGetValue(node.NodeId, out var next) ? next : [];
            var previousLinks = incoming.TryGetValue(node.NodeId, out var previous) ? previous : [];

            builder.AppendLine($"  - key: {nodeKeys[node.NodeId]}");
            builder.AppendLine($"    node_id: {node.NodeId}");
            builder.AppendLine($"    name: {DslValue(node.Name)}");
            builder.AppendLine($"    type: {DslValue(node.NodeType)}");
            builder.AppendLine($"    lane_process_category: {DslValue(laneName)}");
            builder.AppendLine($"    stage_owner_column: {DslValue(stageName)}");
            builder.AppendLine($"    responsibility: action={DslValue(laneName)}, owner={DslValue(stageName)}");
            builder.AppendLine($"    description: {DslValue(node.Description)}");
            builder.AppendLine($"    position: x={node.X}, y={node.Y}");
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
                }
            }
        }
        builder.AppendLine();

        builder.AppendLine("LINKS_PROCESS_FLOW:");
        if (snapshot.Links.Count == 0)
        {
            builder.AppendLine("  - none");
        }
        else
        {
            foreach (var link in snapshot.Links)
            {
                builder.AppendLine($"  - from: {nodeKeys.GetValueOrDefault(link.SourceNodeId, link.SourceNodeId.ToString())}");
                builder.AppendLine($"    to: {nodeKeys.GetValueOrDefault(link.TargetNodeId, link.TargetNodeId.ToString())}");
                builder.AppendLine($"    label: {DslValue(ChooseLinkLabel(link))}");
            }
        }

        if (snapshot.Comments.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("COMMENTS:");
            foreach (var comment in snapshot.Comments)
            {
                builder.AppendLine($"  - node: {(comment.NodeId is Guid nodeId ? nodeKeys.GetValueOrDefault(nodeId, nodeId.ToString()) : "none")}");
                builder.AppendLine($"    text: {DslValue(comment.Text)}");
                builder.AppendLine($"    position: x={comment.X}, y={comment.Y}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static string GetLaneName(ExportNode node, IReadOnlyDictionary<Guid, ExportLane> lanesById)
    {
        return node.LaneId is Guid laneId && lanesById.TryGetValue(laneId, out var lane) ? lane.Name : "未設定";
    }

    private static string GetStageName(ExportNode node, IReadOnlyDictionary<Guid, ExportStage> stagesById)
    {
        return node.StageId is Guid stageId && stagesById.TryGetValue(stageId, out var stage) ? stage.Name : "未設定";
    }

    private static Dictionary<Guid, string> BuildNodeKeys(IReadOnlyList<ExportNode> nodes)
    {
        var keys = new Dictionary<Guid, string>();
        var index = 1;
        foreach (var node in nodes)
        {
            keys[node.NodeId] = $"N{index++}";
        }

        return keys;
    }

    private static string? ChooseLinkLabel(ExportLink link)
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

    private static string BuildMermaidNodeDeclaration(string nodeKey, ExportNode node)
    {
        var label = EscapeMermaidLabel(node.Name);
        return node.NodeType.ToLowerInvariant() switch
        {
            "start" or "end" => $"{nodeKey}([\"{label}\"])",
            "decision" => $"{nodeKey}{{\"{label}\"}}",
            "preparation" => $"{nodeKey}{{{{\"{label}\"}}}}",
            "document" => $"{nodeKey}[/\"{label}\"/]",
            _ => $"{nodeKey}[\"{label}\"]",
        };
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
