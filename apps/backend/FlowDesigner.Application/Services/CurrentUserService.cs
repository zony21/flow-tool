using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FlowDesigner.Application.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public bool IsAuthenticated()
    {
        return httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public Guid? GetCurrentUserId()
    {
        var value = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return null;
        }

        var userName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var displayName = user.FindFirstValue(ClaimTypes.GivenName) ?? userName;
        var email = user.FindFirstValue(ClaimTypes.Email);

        return await Task.FromResult(new CurrentUserDto(userId.Value, userName, displayName, email));
    }
}
