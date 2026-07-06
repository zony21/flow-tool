using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Infrastructure.Data.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!await dbContext.Roles.AnyAsync(cancellationToken))
        {
            var ownerRoleId = Guid.NewGuid();
            var editorRoleId = Guid.NewGuid();
            var viewerRoleId = Guid.NewGuid();

            dbContext.Roles.AddRange(
                new Role { RoleId = ownerRoleId, RoleCode = "OWNER", Name = "Owner" },
                new Role { RoleId = editorRoleId, RoleCode = "EDITOR", Name = "Editor" },
                new Role { RoleId = viewerRoleId, RoleCode = "VIEWER", Name = "Viewer" }
            );

            var permissions = new[]
            {
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = "project.read", Name = "Project Read" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = "project.write", Name = "Project Write" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = "flow.read", Name = "Flow Read" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = "flow.write", Name = "Flow Write" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = "export.execute", Name = "Export Execute" }
            };

            dbContext.Permissions.AddRange(permissions);

            foreach (var permission in permissions)
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RolePermissionId = Guid.NewGuid(),
                    RoleId = ownerRoleId,
                    PermissionId = permission.PermissionId,
                });
            }

            foreach (var permission in permissions.Where(x => x.PermissionCode is "project.read" or "flow.read" or "flow.write" or "export.execute"))
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RolePermissionId = Guid.NewGuid(),
                    RoleId = editorRoleId,
                    PermissionId = permission.PermissionId,
                });
            }

            foreach (var permission in permissions.Where(x => x.PermissionCode is "project.read" or "flow.read"))
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RolePermissionId = Guid.NewGuid(),
                    RoleId = viewerRoleId,
                    PermissionId = permission.PermissionId,
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
            var ownerRole = await dbContext.Roles.FirstAsync(x => x.RoleCode == "OWNER", cancellationToken);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                GitHubId = "demo-owner",
                UserName = "demo-owner",
                DisplayName = "Demo Owner",
                Email = "demo@example.com",
                CreatedAtUtc = DateTime.UtcNow,
            };

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = "Demo Project",
                Description = "Seed project for development",
                CreatedAtUtc = DateTime.UtcNow,
            };

            dbContext.Users.Add(user);
            dbContext.Projects.Add(project);
            dbContext.ProjectMembers.Add(new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = project.ProjectId,
                UserId = user.UserId,
                RoleId = ownerRole.RoleId,
                JoinedAtUtc = DateTime.UtcNow,
            });

            dbContext.ProjectSettings.Add(new FlowDesigner.Domain.Entities.Settings.ProjectSetting
            {
                ProjectSettingId = Guid.NewGuid(),
                ProjectId = project.ProjectId,
                AutoSaveEnabled = true,
                AutoSaveIntervalSec = 30,
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
