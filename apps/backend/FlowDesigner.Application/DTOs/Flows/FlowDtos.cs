namespace FlowDesigner.Application.DTOs.Flows;

public sealed record FlowSummaryDto(
    Guid FlowId,
    Guid ProjectId,
    string Name,
    string? Description,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record FlowDetailDto(
    Guid FlowId,
    Guid ProjectId,
    string Name,
    string? Description,
    int SortOrder,
    IReadOnlyList<LaneDto> Lanes,
    IReadOnlyList<StageDto> Stages,
    IReadOnlyList<NodeDto> Nodes,
    IReadOnlyList<LinkDto> Links,
    IReadOnlyList<CommentDto> Comments,
    IReadOnlyList<MetadataDto> Metadata);

public sealed record LaneDto(
    Guid LaneId,
    Guid FlowId,
    string Name,
    int SortOrder);

public sealed record StageDto(
    Guid StageId,
    Guid FlowId,
    string Name,
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
    double Y);

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
    string? Description);

public sealed record UpdateFlowRequest(
    string Name,
    string? Description);
