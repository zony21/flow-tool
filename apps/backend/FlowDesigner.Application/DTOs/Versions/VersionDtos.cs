namespace FlowDesigner.Application.DTOs.Versions;

public sealed record FlowVersionSummaryDto(
    Guid VersionId,
    Guid FlowId,
    int VersionNumber,
    string DisplayVersion,
    string? Comment,
    DateTime CreatedAtUtc,
    int NodeCount,
    int LinkCount,
    int CommentCount);

public sealed record CreateFlowVersionRequest(
    string? Comment);
