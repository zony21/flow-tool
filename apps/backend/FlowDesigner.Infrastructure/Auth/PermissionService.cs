using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Infrastructure.Auth;

public sealed class PermissionService(AppDbContext dbContext) : IPermissionService
{
    public async Task<bool> CanAsync(Guid userId, Guid projectId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, projectId, cancellationToken);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var query =
            from member in dbContext.ProjectMembers.AsNoTracking()
            join role in dbContext.Roles.AsNoTracking() on member.RoleId equals role.RoleId
            join rolePermission in dbContext.RolePermissions.AsNoTracking() on role.RoleId equals rolePermission.RoleId
            join permission in dbContext.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.PermissionId
            where member.UserId == userId && member.ProjectId == projectId
            select permission.PermissionCode;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<ProjectPermissionDto?> GetProjectPermissionAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var member = await dbContext.ProjectMembers
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProjectId == projectId, cancellationToken);

        if (member is null)
        {
            return null;
        }

        var permissions = await GetPermissionsAsync(userId, projectId, cancellationToken);
        return new ProjectPermissionDto(projectId, member.Role.RoleCode, permissions);
    }
}
