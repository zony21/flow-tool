namespace FlowDesigner.Application.DTOs.Flows;

public sealed record FlowSummaryDto(
    Guid FlowId,
    Guid ProjectId,
    string Name,
    string FlowType,
    string? Description,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record FlowDetailDto(
    Guid FlowId,
    Guid ProjectId,
    string Name,
    string FlowType,
    string? Description,
    int SortOrder,
    int CurrentRevision,
    IReadOnlyList<LaneDto> Lanes,
    IReadOnlyList<StageDto> Stages,
    IReadOnlyList<NodeDto> Nodes,
    IReadOnlyList<LinkDto> Links,
    IReadOnlyList<CommentDto> Comments,
    IReadOnlyList<MetadataDto> Metadata);

public sealed record SaveFlowStructureRequest(
    Guid FlowId,
    int ClientRevision,
    IReadOnlyList<SaveLaneRequest>? Lanes,
    IReadOnlyList<SaveStageRequest>? Stages,
    IReadOnlyList<SaveNodeRequest>? Nodes,
    IReadOnlyList<SaveLinkRequest>? Links,
    IReadOnlyList<SaveCommentRequest>? Comments,
    bool CreateVersion,
    string? ChangeSummary);

public sealed record SaveFlowStructureResponse(
    Guid FlowId,
    int ServerRevision,
    DateTime UpdatedAtUtc);

public sealed record SaveLaneRequest(
    Guid LaneId,
    string Name,
    int SortOrder);

public sealed record SaveStageRequest(
    Guid StageId,
    string Name,
    string? StageType,
    int SortOrder);

public sealed record SaveNodeRequest(
    Guid NodeId,
    Guid? LaneId,
    Guid? StageId,
    string NodeType,
    string Name,
    string? Description,
    double X,
    double Y,
    Guid? CommandId = null,
    Guid? LocationId = null,
    Guid? EquipmentId = null,
    string? RwType = null);

public sealed record SaveLinkRequest(
    Guid LinkId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label,
    string? Condition);

public sealed record SaveCommentRequest(
    Guid CommentId,
    Guid? NodeId,
    string Text,
    double X,
    double Y);

public sealed record LaneDto(
    Guid LaneId,
    Guid FlowId,
    string Name,
    int SortOrder);

public sealed record StageDto(
    Guid StageId,
    Guid FlowId,
    string Name,
    string StageType,
    int SortOrder);

public sealed record NodeDto(
    Guid NodeId,
    Guid FlowId,
    Guid? LaneId,
    Guid? StageId,
    string NodeType,
    string Name,
    string? Description,
    double X,
    double Y,
    Guid? CommandId = null,
    Guid? LocationId = null,
    Guid? EquipmentId = null,
    string? RwType = null);

public sealed record LinkDto(
    Guid LinkId,
    Guid FlowId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label,
    string? Condition);

public sealed record CommentDto(
    Guid CommentId,
    Guid FlowId,
    Guid? NodeId,
    string Text,
    double X,
    double Y);

public sealed record MetadataDto(
    Guid MetadataId,
    Guid FlowId,
    string MetaKey,
    string MetaValue);

public sealed record CreateFlowRequest(
    string Name,
    string? Description,
    string? FlowType = null);

public sealed record UpdateFlowRequest(
    string Name,
    string? Description,
    string? FlowType = null);

public sealed record DuplicateFlowRequest(
    string? Name);
