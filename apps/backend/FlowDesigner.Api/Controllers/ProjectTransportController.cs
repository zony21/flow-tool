using FlowDesigner.Api.Attributes;
using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Transport;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Application.Security;
using FlowDesigner.Domain.Entities.Transport;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/transport")]
[Authorize]
public sealed class ProjectTransportController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("locations")]
    [RequirePermission(PermissionCodes.FlowRead)]
    public async Task<ActionResult<IReadOnlyList<TransportLocationDto>>> ListLocations(Guid projectId, CancellationToken cancellationToken)
    {
        var items = await dbContext.TransportLocations
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new TransportLocationDto(x.LocationId, x.ProjectId, x.Name, x.LocationType, x.Description, x.SortOrder, x.IsDeleted, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("locations")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<TransportLocationDto>> CreateLocation(Guid projectId, [FromBody] SaveTransportLocationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.LocationType))
        {
            return ApiError.BadRequest<TransportLocationDto>(this, "Location name and locationType are required.");
        }

        var projectExists = await dbContext.Projects.AnyAsync(x => x.ProjectId == projectId, cancellationToken);
        if (!projectExists)
        {
            return ApiError.NotFound<TransportLocationDto>(this, "Project was not found.");
        }

        var now = DateTime.UtcNow;
        var currentUserId = currentUserService.GetCurrentUserId();
        var sortOrder = request.SortOrder ?? await NextLocationSortOrderAsync(projectId, cancellationToken);
        var entity = new TransportLocation
        {
            LocationId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name.Trim(),
            LocationType = request.LocationType.Trim(),
            Description = request.Description,
            SortOrder = sortOrder,
            CreatedAtUtc = now,
            CreatedByUserId = currentUserId,
            UpdatedAtUtc = now,
            UpdatedByUserId = currentUserId,
        };

        dbContext.TransportLocations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("locations/{locationId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<TransportLocationDto>> UpdateLocation(Guid projectId, Guid locationId, [FromBody] SaveTransportLocationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.LocationType))
        {
            return ApiError.BadRequest<TransportLocationDto>(this, "Location name and locationType are required.");
        }

        var entity = await dbContext.TransportLocations.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.LocationId == locationId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return ApiError.NotFound<TransportLocationDto>(this, "Location was not found in this project.");
        }

        entity.Name = request.Name.Trim();
        entity.LocationType = request.LocationType.Trim();
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUserService.GetCurrentUserId();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpDelete("locations/{locationId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<IActionResult> DeleteLocation(Guid projectId, Guid locationId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportLocations.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.LocationId == locationId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Location was not found in this project."));
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.DeletedByUserId = currentUserService.GetCurrentUserId();
        entity.UpdatedAtUtc = entity.DeletedAtUtc.Value;
        entity.UpdatedByUserId = entity.DeletedByUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("equipments")]
    [RequirePermission(PermissionCodes.FlowRead)]
    public async Task<ActionResult<IReadOnlyList<TransportEquipmentDto>>> ListEquipments(Guid projectId, CancellationToken cancellationToken)
    {
        var items = await dbContext.TransportEquipments
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new TransportEquipmentDto(x.EquipmentId, x.ProjectId, x.Name, x.Category, x.Description, x.SortOrder, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("equipments")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<TransportEquipmentDto>> CreateEquipment(Guid projectId, [FromBody] SaveTransportEquipmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category))
        {
            return ApiError.BadRequest<TransportEquipmentDto>(this, "Equipment name and category are required.");
        }

        var projectExists = await dbContext.Projects.AnyAsync(x => x.ProjectId == projectId, cancellationToken);
        if (!projectExists)
        {
            return ApiError.NotFound<TransportEquipmentDto>(this, "Project was not found.");
        }

        var now = DateTime.UtcNow;
        var currentUserId = currentUserService.GetCurrentUserId();
        var sortOrder = request.SortOrder ?? await NextEquipmentSortOrderAsync(projectId, cancellationToken);
        var entity = new TransportEquipment
        {
            EquipmentId = Guid.NewGuid(),
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Description = request.Description,
            SortOrder = sortOrder,
            CreatedAtUtc = now,
            CreatedByUserId = currentUserId,
            UpdatedAtUtc = now,
            UpdatedByUserId = currentUserId,
        };

        dbContext.TransportEquipments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("equipments/{equipmentId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<ActionResult<TransportEquipmentDto>> UpdateEquipment(Guid projectId, Guid equipmentId, [FromBody] SaveTransportEquipmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category))
        {
            return ApiError.BadRequest<TransportEquipmentDto>(this, "Equipment name and category are required.");
        }

        var entity = await dbContext.TransportEquipments.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.EquipmentId == equipmentId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return ApiError.NotFound<TransportEquipmentDto>(this, "Equipment was not found in this project.");
        }

        entity.Name = request.Name.Trim();
        entity.Category = request.Category.Trim();
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUserService.GetCurrentUserId();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpDelete("equipments/{equipmentId:guid}")]
    [RequirePermission(PermissionCodes.FlowUpdate)]
    public async Task<IActionResult> DeleteEquipment(Guid projectId, Guid equipmentId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportEquipments.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.EquipmentId == equipmentId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Equipment was not found in this project."));
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.DeletedByUserId = currentUserService.GetCurrentUserId();
        entity.UpdatedAtUtc = entity.DeletedAtUtc.Value;
        entity.UpdatedByUserId = entity.DeletedByUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<int> NextLocationSortOrderAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return (await dbContext.TransportLocations.Where(x => x.ProjectId == projectId && !x.IsDeleted).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0) + 1;
    }

    private async Task<int> NextEquipmentSortOrderAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return (await dbContext.TransportEquipments.Where(x => x.ProjectId == projectId && !x.IsDeleted).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0) + 1;
    }

    private static TransportLocationDto ToDto(TransportLocation entity)
    {
        return new TransportLocationDto(entity.LocationId, entity.ProjectId, entity.Name, entity.LocationType, entity.Description, entity.SortOrder, entity.IsDeleted, entity.CreatedAtUtc, entity.UpdatedAtUtc);
    }

    private static TransportEquipmentDto ToDto(TransportEquipment entity)
    {
        return new TransportEquipmentDto(entity.EquipmentId, entity.ProjectId, entity.Name, entity.Category, entity.Description, entity.SortOrder, entity.CreatedAtUtc, entity.UpdatedAtUtc);
    }
}
