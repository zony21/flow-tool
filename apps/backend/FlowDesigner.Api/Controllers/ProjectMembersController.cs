using FlowDesigner.Api.Attributes;
using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/members")]
public sealed class ProjectMembersController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [RequirePermission("project.read")]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberDto>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var members = await dbContext.ProjectMembers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.JoinedAtUtc)
            .Select(x => new ProjectMemberDto(
                x.ProjectMemberId,
                x.ProjectId,
                x.UserId,
                x.User.UserName,
                x.User.DisplayName,
                x.User.Email,
                x.Role.RoleCode,
                x.Role.Name))
            .ToListAsync(cancellationToken);

        return Ok(members);
    }

    public sealed record InviteRequest(string Email, string RoleCode);
    public sealed record ChangeRoleRequest(string RoleCode);

    [HttpPost("invite")]
    [RequirePermission("project.write")]
    public async Task<IActionResult> Invite(Guid projectId, [FromBody] InviteRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleCode == request.RoleCode, cancellationToken);
        if (role is null)
        {
            return BadRequest(new { message = "Invalid roleCode." });
        }

        var invite = new ProjectInvite
        {
            ProjectInviteId = Guid.NewGuid(),
            ProjectId = projectId,
            InvitedByUserId = currentUserId.Value,
            Email = request.Email,
            Status = "pending",
            CreatedAtUtc = DateTime.UtcNow,
        };

        dbContext.ProjectInvites.Add(invite);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpPatch("{memberId:guid}/role")]
    [RequirePermission("project.write")]
    public async Task<IActionResult> ChangeRole(Guid projectId, Guid memberId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var member = await dbContext.ProjectMembers.Include(x => x.Role).FirstOrDefaultAsync(x => x.ProjectMemberId == memberId && x.ProjectId == projectId, cancellationToken);
        if (member is null)
        {
            return NotFound();
        }

        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleCode == request.RoleCode, cancellationToken);
        if (role is null)
        {
            return BadRequest(new { message = "Invalid roleCode." });
        }

        var ownerRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleCode == "OWNER", cancellationToken);
        if (ownerRole is not null && member.RoleId == ownerRole.RoleId && request.RoleCode != "OWNER")
        {
            var ownerCount = await dbContext.ProjectMembers.CountAsync(x => x.ProjectId == projectId && x.RoleId == ownerRole.RoleId, cancellationToken);
            if (ownerCount <= 1)
            {
                return Conflict(new { message = "Cannot demote the last owner." });
            }
        }

        member.RoleId = role.RoleId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpDelete("{memberId:guid}")]
    [RequirePermission("project.write")]
    public async Task<IActionResult> Remove(Guid projectId, Guid memberId, CancellationToken cancellationToken)
    {
        var member = await dbContext.ProjectMembers.Include(x => x.Role).FirstOrDefaultAsync(x => x.ProjectMemberId == memberId && x.ProjectId == projectId, cancellationToken);
        if (member is null)
        {
            return NotFound();
        }

        var ownerRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleCode == "OWNER", cancellationToken);
        if (ownerRole is not null && member.RoleId == ownerRole.RoleId)
        {
            var ownerCount = await dbContext.ProjectMembers.CountAsync(x => x.ProjectId == projectId && x.RoleId == ownerRole.RoleId, cancellationToken);
            if (ownerCount <= 1)
            {
                return Conflict(new { message = "Cannot remove the last owner." });
            }
        }

        dbContext.ProjectMembers.Remove(member);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }
}
