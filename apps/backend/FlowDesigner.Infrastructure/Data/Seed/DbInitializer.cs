using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Application.Security;
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
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.ProjectRead, Name = "Project Read" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.ProjectUpdate, Name = "Project Update" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.FlowRead, Name = "Flow Read" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.FlowUpdate, Name = "Flow Update" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.VersionRead, Name = "Version Read" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.VersionCreate, Name = "Version Create" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.NodeUpdate, Name = "Node Update" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.LinkUpdate, Name = "Link Update" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.CommentUpdate, Name = "Comment Update" },
                new Permission { PermissionId = Guid.NewGuid(), PermissionCode = PermissionCodes.ExportExecute, Name = "Export Execute" }
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

            foreach (var permission in permissions.Where(x => x.PermissionCode is PermissionCodes.ProjectRead or PermissionCodes.FlowRead or PermissionCodes.FlowUpdate or PermissionCodes.VersionRead or PermissionCodes.VersionCreate or PermissionCodes.NodeUpdate or PermissionCodes.LinkUpdate or PermissionCodes.CommentUpdate or PermissionCodes.ExportExecute))
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RolePermissionId = Guid.NewGuid(),
                    RoleId = editorRoleId,
                    PermissionId = permission.PermissionId,
                });
            }

            foreach (var permission in permissions.Where(x => x.PermissionCode is PermissionCodes.ProjectRead or PermissionCodes.FlowRead or PermissionCodes.VersionRead))
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
