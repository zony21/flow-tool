namespace FlowDesigner.Application.DTOs.Auth;

public sealed record CurrentUserDto(
    Guid UserId,
    string UserName,
    string DisplayName,
    string? Email
);
