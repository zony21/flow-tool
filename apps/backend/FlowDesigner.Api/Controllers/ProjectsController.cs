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
        var project = await dbContext.Projects.FirstOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);
        if (project is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Project was not found."));
        }

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
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
