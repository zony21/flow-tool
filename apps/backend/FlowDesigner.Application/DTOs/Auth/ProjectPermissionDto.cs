namespace FlowDesigner.Application.DTOs.Auth;

public sealed record ProjectPermissionDto(
    Guid ProjectId,
    string RoleCode,
    IReadOnlyList<string> Permissions
);
