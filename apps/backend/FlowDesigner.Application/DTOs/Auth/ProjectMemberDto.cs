namespace FlowDesigner.Application.DTOs.Auth;

public sealed record ProjectMemberDto(
    Guid ProjectMemberId,
    Guid ProjectId,
    Guid UserId,
    string UserName,
    string DisplayName,
    string? Email,
    string RoleCode,
    string RoleName
);
