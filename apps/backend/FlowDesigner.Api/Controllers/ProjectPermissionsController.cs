using FlowDesigner.Api.Attributes;
using FlowDesigner.Application.DTOs.Auth;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/permissions")]
public sealed class ProjectPermissionsController(
    ICurrentUserService currentUserService,
    IPermissionService permissionService) : ControllerBase
{
    [HttpGet("me")]
    [RequirePermission("project.read")]
    public async Task<ActionResult<ProjectPermissionDto>> Me(Guid projectId, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var permission = await permissionService.GetProjectPermissionAsync(userId.Value, projectId, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }
}
