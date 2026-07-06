namespace FlowDesigner.Application.DTOs.Projects;

public sealed record ProjectSummaryDto(
    Guid ProjectId,
    string Name,
    string? Description,
    DateTime CreatedAtUtc);

public sealed record ProjectDetailDto(
    Guid ProjectId,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateProjectRequest(
    string Name,
    string? Description);

public sealed record UpdateProjectRequest(
    string Name,
    string? Description);
