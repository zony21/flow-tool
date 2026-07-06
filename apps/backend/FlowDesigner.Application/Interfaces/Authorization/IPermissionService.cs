using FlowDesigner.Application.DTOs.Auth;

namespace FlowDesigner.Application.Interfaces.Authorization;

public interface IPermissionService
{
    Task<bool> CanAsync(Guid userId, Guid projectId, string permissionCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectPermissionDto?> GetProjectPermissionAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);
}
