using FlowDesigner.Application.DTOs.Auth;

namespace FlowDesigner.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Guid? GetCurrentUserId();
    bool IsAuthenticated();
}
