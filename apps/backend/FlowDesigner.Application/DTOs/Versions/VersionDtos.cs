namespace FlowDesigner.Application.DTOs.Versions;

public sealed record FlowVersionSummaryDto(
    Guid VersionId,
    Guid FlowId,
    int VersionNumber,
    string DisplayVersion,
    string? Comment,
    string? CreatedByDisplayName,
    DateTime CreatedAtUtc,
    int NodeCount,
    int LinkCount,
    int CommentCount);

public sealed record CreateFlowVersionRequest(
    string? Comment);

public sealed record RestoreFlowVersionResponse(
    Guid FlowId,
    Guid RestoredVersionId,
    int CurrentRevision);

public sealed record FlowVersionCompareResponse(
    Guid LeftVersionId,
    Guid RightVersionId,
    IReadOnlyList<VersionDiffItemDto> LaneDiffs,
    IReadOnlyList<VersionDiffItemDto> StageDiffs,
    IReadOnlyList<VersionDiffItemDto> NodeDiffs,
    IReadOnlyList<VersionDiffItemDto> LinkDiffs,
    IReadOnlyList<VersionDiffItemDto> CommentDiffs);

public sealed record VersionDiffItemDto(
    string EntityType,
    string EntityId,
    string ChangeType,
    string Label);
