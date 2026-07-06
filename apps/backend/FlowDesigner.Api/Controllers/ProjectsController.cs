using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Projects;
using FlowDesigner.Application.Security;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Domain.Entities.Settings;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.ProjectMembers.Any(member => member.UserId == userId.Value))
            .OrderBy(project => project.CreatedAtUtc)
            .Select(project => new ProjectSummaryDto(
                project.ProjectId,
                project.Name,
                project.Description,
                project.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDetailDto>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiError.BadRequest<ProjectDetailDto>(this, "Project name is required.", "name");
        }

        var ownerRole = await dbContext.Roles.FirstOrDefaultAsync(role => role.RoleCode == "OWNER", cancellationToken);
        if (ownerRole is null)
        {
            return ApiError.Internal(this, "OWNER role is not seeded.");
        }

        var now = DateTime.UtcNow;
        var project = new Project
        {
            ProjectId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            CreatedAtUtc = now,
        };

        dbContext.Projects.Add(project);
        dbContext.ProjectMembers.Add(new ProjectMember
        {
            ProjectMemberId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            UserId = userId.Value,
            RoleId = ownerRole.RoleId,
            JoinedAtUtc = now,
        });
        dbContext.ProjectSettings.Add(new ProjectSetting
        {
            ProjectSettingId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            AutoSaveEnabled = true,
            AutoSaveIntervalSec = 30,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { projectId = project.ProjectId }, ToDetailDto(project));
    }

    [HttpGet("{projectId:guid}")]
    [RequirePermission(PermissionCodes.ProjectRead)]
    public async Task<ActionResult<ProjectDetailDto>> Get(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);

        return project is null
            ? ApiError.NotFound<ProjectDetailDto>(this, "Project was not found.")
            : Ok(ToDetailDto(project));
    }

    [HttpPut("{projectId:guid}")]
    [RequirePermission(PermissionCodes.ProjectUpdate)]
    public async Task<ActionResult<ProjectDetailDto>> Update(Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiError.BadRequest<ProjectDetailDto>(this, "Project name is required.", "name");
        }

        var project = await dbContext.Projects.FirstOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
        if (project is null)
        {
            return ApiError.NotFound<ProjectDetailDto>(this, "Project was not found.");
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDetailDto(project));
    }

    [HttpDelete("{projectId:guid}")]
    [RequirePermission(PermissionCodes.ProjectUpdate)]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(ApiError.Create(HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
        }

        var projectExists = await dbContext.Projects.AnyAsync(x => x.ProjectId == projectId, cancellationToken);
        if (!projectExists)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Project was not found."));
        }

        try
        {
            await DeleteProjectPhysicallyAsync(projectId, cancellationToken);
        }
        catch (Exception)
        {
            await RemoveCurrentUserMembershipAsync(projectId, userId.Value, cancellationToken);
        }

        return NoContent();
    }

    private async Task DeleteProjectPhysicallyAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var flowIds = await dbContext.Flows
            .Where(flow => flow.ProjectId == projectId)
            .Select(flow => flow.FlowId)
            .ToListAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (flowIds.Count > 0)
            {
                await dbContext.Versions.Where(version => flowIds.Contains(version.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.MetadataItems.Where(metadata => flowIds.Contains(metadata.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Comments.Where(comment => flowIds.Contains(comment.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Links.Where(link => flowIds.Contains(link.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Nodes.Where(node => flowIds.Contains(node.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Stages.Where(stage => flowIds.Contains(stage.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Lanes.Where(lane => flowIds.Contains(lane.FlowId)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Images.Where(image => image.ProjectId == projectId || image.FlowId != null && flowIds.Contains(image.FlowId.Value)).ExecuteDeleteAsync(cancellationToken);
                await dbContext.Flows.Where(flow => flow.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                await dbContext.Images.Where(image => image.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            }

            await dbContext.ProjectInvites.Where(invite => invite.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            await dbContext.ProjectMembers.Where(member => member.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            await dbContext.ProjectSettings.Where(setting => setting.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            await dbContext.AuditLogs.Where(log => log.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);
            await dbContext.Projects.Where(item => item.ProjectId == projectId).ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task RemoveCurrentUserMembershipAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var membership = await dbContext.ProjectMembers
            .FirstOrDefaultAsync(member => member.ProjectId == projectId && member.UserId == userId, cancellationToken);

        if (membership is null)
        {
            return;
        }

        dbContext.ProjectMembers.Remove(membership);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProjectDetailDto ToDetailDto(Project project)
    {
        return new ProjectDetailDto(
            project.ProjectId,
            project.Name,
            project.Description,
            project.CreatedAtUtc,
            null);
    }
}
