using FlowDesigner.Api.Common;
using FlowDesigner.Application.DTOs.Transport;
using FlowDesigner.Application.Interfaces.Services;
using FlowDesigner.Domain.Entities.Transport;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Api.Controllers;

[ApiController]
[Route("api/transport")]
[Authorize]
public sealed class TransportMastersController(
    AppDbContext dbContext,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("manufacturers")]
    public async Task<ActionResult<IReadOnlyList<TransportManufacturerDto>>> ListManufacturers(CancellationToken cancellationToken)
    {
        var items = await dbContext.TransportManufacturers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new TransportManufacturerDto(x.ManufacturerId, x.Name, x.VehicleType, x.Description, x.SortOrder, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("manufacturers")]
    public async Task<ActionResult<TransportManufacturerDto>> CreateManufacturer([FromBody] SaveTransportManufacturerRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.VehicleType))
        {
            return ApiError.BadRequest<TransportManufacturerDto>(this, "Manufacturer name and vehicleType are required.");
        }

        var now = DateTime.UtcNow;
        var currentUserId = currentUserService.GetCurrentUserId();
        var sortOrder = request.SortOrder ?? await NextManufacturerSortOrderAsync(cancellationToken);
        var entity = new TransportManufacturer
        {
            ManufacturerId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            VehicleType = request.VehicleType.Trim(),
            Description = request.Description,
            SortOrder = sortOrder,
            CreatedAtUtc = now,
            CreatedByUserId = currentUserId,
            UpdatedAtUtc = now,
            UpdatedByUserId = currentUserId,
        };

        dbContext.TransportManufacturers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("manufacturers/{manufacturerId:guid}")]
    public async Task<ActionResult<TransportManufacturerDto>> UpdateManufacturer(Guid manufacturerId, [FromBody] SaveTransportManufacturerRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.VehicleType))
        {
            return ApiError.BadRequest<TransportManufacturerDto>(this, "Manufacturer name and vehicleType are required.");
        }

        var entity = await dbContext.TransportManufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == manufacturerId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return ApiError.NotFound<TransportManufacturerDto>(this, "Manufacturer was not found.");
        }

        entity.Name = request.Name.Trim();
        entity.VehicleType = request.VehicleType.Trim();
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUserService.GetCurrentUserId();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpDelete("manufacturers/{manufacturerId:guid}")]
    public async Task<IActionResult> DeleteManufacturer(Guid manufacturerId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportManufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == manufacturerId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Manufacturer was not found."));
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.DeletedByUserId = currentUserService.GetCurrentUserId();
        entity.UpdatedAtUtc = entity.DeletedAtUtc.Value;
        entity.UpdatedByUserId = entity.DeletedByUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("commands")]
    public async Task<ActionResult<IReadOnlyList<TransportCommandDto>>> ListCommands([FromQuery] Guid? manufacturerId, CancellationToken cancellationToken)
    {
        var query = dbContext.TransportCommands.AsNoTracking().Where(x => !x.IsDeleted);
        if (manufacturerId.HasValue)
        {
            query = query.Where(x => x.ManufacturerId == manufacturerId.Value);
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CommandName)
            .Select(x => new TransportCommandDto(x.CommandId, x.ManufacturerId, x.CommandName, x.ProcessType, x.Description, x.SortOrder, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("commands")]
    public async Task<ActionResult<TransportCommandDto>> CreateCommand([FromBody] SaveTransportCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CommandName) || string.IsNullOrWhiteSpace(request.ProcessType))
        {
            return ApiError.BadRequest<TransportCommandDto>(this, "CommandName and processType are required.");
        }

        var manufacturerExists = await dbContext.TransportManufacturers.AnyAsync(x => x.ManufacturerId == request.ManufacturerId && !x.IsDeleted, cancellationToken);
        if (!manufacturerExists)
        {
            return ApiError.NotFound<TransportCommandDto>(this, "Manufacturer was not found.");
        }

        var now = DateTime.UtcNow;
        var currentUserId = currentUserService.GetCurrentUserId();
        var sortOrder = request.SortOrder ?? await NextCommandSortOrderAsync(request.ManufacturerId, cancellationToken);
        var entity = new TransportCommand
        {
            CommandId = Guid.NewGuid(),
            ManufacturerId = request.ManufacturerId,
            CommandName = request.CommandName.Trim(),
            ProcessType = request.ProcessType.Trim(),
            Description = request.Description,
            SortOrder = sortOrder,
            CreatedAtUtc = now,
            CreatedByUserId = currentUserId,
            UpdatedAtUtc = now,
            UpdatedByUserId = currentUserId,
        };

        dbContext.TransportCommands.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("commands/{commandId:guid}")]
    public async Task<ActionResult<TransportCommandDto>> UpdateCommand(Guid commandId, [FromBody] SaveTransportCommandRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CommandName) || string.IsNullOrWhiteSpace(request.ProcessType))
        {
            return ApiError.BadRequest<TransportCommandDto>(this, "CommandName and processType are required.");
        }

        var manufacturerExists = await dbContext.TransportManufacturers.AnyAsync(x => x.ManufacturerId == request.ManufacturerId && !x.IsDeleted, cancellationToken);
        if (!manufacturerExists)
        {
            return ApiError.NotFound<TransportCommandDto>(this, "Manufacturer was not found.");
        }

        var entity = await dbContext.TransportCommands.FirstOrDefaultAsync(x => x.CommandId == commandId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return ApiError.NotFound<TransportCommandDto>(this, "Command was not found.");
        }

        entity.ManufacturerId = request.ManufacturerId;
        entity.CommandName = request.CommandName.Trim();
        entity.ProcessType = request.ProcessType.Trim();
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder ?? entity.SortOrder;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUserService.GetCurrentUserId();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpDelete("commands/{commandId:guid}")]
    public async Task<IActionResult> DeleteCommand(Guid commandId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportCommands.FirstOrDefaultAsync(x => x.CommandId == commandId && !x.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Command was not found."));
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.DeletedByUserId = currentUserService.GetCurrentUserId();
        entity.UpdatedAtUtc = entity.DeletedAtUtc.Value;
        entity.UpdatedByUserId = entity.DeletedByUserId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("vehicle-models")]
    public async Task<ActionResult<IReadOnlyList<TransportVehicleModelDto>>> ListVehicleModels(
        [FromQuery] Guid? manufacturerId,
        [FromQuery] string? vehicleType,
        [FromQuery] bool includeInactive = true,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.TransportVehicleModels.AsNoTracking().Where(x => !x.IsDeleted);
        if (manufacturerId.HasValue) query = query.Where(x => x.ManufacturerId == manufacturerId.Value);
        if (!string.IsNullOrWhiteSpace(vehicleType)) query = query.Where(x => x.VehicleType == vehicleType.Trim().ToUpper());
        if (!includeInactive) query = query.Where(x => x.IsActive);
        var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.ModelName)
            .Select(x => new TransportVehicleModelDto(x.VehicleModelId, x.ManufacturerId, x.Manufacturer.Name, x.VehicleType, x.ModelCode, x.ModelName, x.Description, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("vehicle-models")]
    public async Task<ActionResult<TransportVehicleModelDto>> CreateVehicleModel([FromBody] SaveTransportVehicleModelRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateVehicleModelAsync(request, null, cancellationToken);
        if (error is not null) return UnprocessableEntity(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, error));
        var now = DateTime.UtcNow;
        var entity = new TransportVehicleModel
        {
            VehicleModelId = Guid.NewGuid(), ManufacturerId = request.ManufacturerId, VehicleType = request.VehicleType.Trim().ToUpper(),
            ModelCode = request.ModelCode.Trim().ToUpper(), ModelName = request.ModelName.Trim(), Description = request.Description,
            SortOrder = request.SortOrder ?? await NextVehicleModelSortOrderAsync(cancellationToken), IsActive = request.IsActive,
            CreatedAtUtc = now, UpdatedAtUtc = now, CreatedByUserId = currentUserService.GetCurrentUserId(), UpdatedByUserId = currentUserService.GetCurrentUserId(),
        };
        dbContext.TransportVehicleModels.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await VehicleModelDtoAsync(entity.VehicleModelId, cancellationToken));
    }

    [HttpPut("vehicle-models/{vehicleModelId:guid}")]
    public async Task<ActionResult<TransportVehicleModelDto>> UpdateVehicleModel(Guid vehicleModelId, [FromBody] SaveTransportVehicleModelRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportVehicleModels.FirstOrDefaultAsync(x => x.VehicleModelId == vehicleModelId && !x.IsDeleted, cancellationToken);
        if (entity is null) return ApiError.NotFound<TransportVehicleModelDto>(this, "Vehicle Model was not found.");
        var error = await ValidateVehicleModelAsync(request, vehicleModelId, cancellationToken);
        if (error is not null) return UnprocessableEntity(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, error));
        entity.ManufacturerId = request.ManufacturerId; entity.VehicleType = request.VehicleType.Trim().ToUpper(); entity.ModelCode = request.ModelCode.Trim().ToUpper();
        entity.ModelName = request.ModelName.Trim(); entity.Description = request.Description; entity.SortOrder = request.SortOrder ?? entity.SortOrder; entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow; entity.UpdatedByUserId = currentUserService.GetCurrentUserId();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await VehicleModelDtoAsync(vehicleModelId, cancellationToken));
    }

    [HttpDelete("vehicle-models/{vehicleModelId:guid}")]
    public async Task<IActionResult> DeleteVehicleModel(Guid vehicleModelId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TransportVehicleModels.FirstOrDefaultAsync(x => x.VehicleModelId == vehicleModelId && !x.IsDeleted, cancellationToken);
        if (entity is null) return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Vehicle Model was not found."));
        if (await dbContext.Nodes.AnyAsync(x => x.VehicleModelId == vehicleModelId, cancellationToken))
            return Conflict(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, "Vehicle Model is assigned to a Node."));
        entity.IsDeleted = true; entity.DeletedAtUtc = DateTime.UtcNow; entity.DeletedByUserId = currentUserService.GetCurrentUserId(); entity.UpdatedAtUtc = entity.DeletedAtUtc.Value; entity.UpdatedByUserId = entity.DeletedByUserId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<string?> ValidateVehicleModelAsync(SaveTransportVehicleModelRequest request, Guid? currentId, CancellationToken cancellationToken)
    {
        var type = request.VehicleType?.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(request.ModelCode) || string.IsNullOrWhiteSpace(request.ModelName)) return "ModelCode and ModelName are required.";
        if (type is not ("AGF" or "AGV")) return "VehicleType must be AGF or AGV.";
        var manufacturer = await dbContext.TransportManufacturers.AsNoTracking().FirstOrDefaultAsync(x => x.ManufacturerId == request.ManufacturerId && !x.IsDeleted, cancellationToken);
        if (manufacturer is null) return "Manufacturer was not found.";
        if (!string.Equals(manufacturer.VehicleType, type, StringComparison.OrdinalIgnoreCase)) return "VehicleType must match Manufacturer VehicleType.";
        var code = request.ModelCode.Trim().ToUpper();
        if (await dbContext.TransportVehicleModels.AnyAsync(x => x.ManufacturerId == request.ManufacturerId && x.VehicleType == type && x.ModelCode == code && !x.IsDeleted && x.VehicleModelId != currentId, cancellationToken))
            return "ModelCode already exists for this Manufacturer and VehicleType.";
        return null;
    }

    private async Task<int> NextVehicleModelSortOrderAsync(CancellationToken cancellationToken) =>
        (await dbContext.TransportVehicleModels.Where(x => !x.IsDeleted).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0) + 1;

    private async Task<TransportVehicleModelDto> VehicleModelDtoAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.TransportVehicleModels.AsNoTracking().Where(x => x.VehicleModelId == id)
            .Select(x => new TransportVehicleModelDto(x.VehicleModelId, x.ManufacturerId, x.Manufacturer.Name, x.VehicleType, x.ModelCode, x.ModelName, x.Description, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc))
            .SingleAsync(cancellationToken);

    private async Task<int> NextManufacturerSortOrderAsync(CancellationToken cancellationToken)
    {
        return (await dbContext.TransportManufacturers.Where(x => !x.IsDeleted).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0) + 1;
    }

    private async Task<int> NextCommandSortOrderAsync(Guid manufacturerId, CancellationToken cancellationToken)
    {
        return (await dbContext.TransportCommands.Where(x => x.ManufacturerId == manufacturerId && !x.IsDeleted).Select(x => (int?)x.SortOrder).MaxAsync(cancellationToken) ?? 0) + 1;
    }

    private static TransportManufacturerDto ToDto(TransportManufacturer entity)
    {
        return new TransportManufacturerDto(entity.ManufacturerId, entity.Name, entity.VehicleType, entity.Description, entity.SortOrder, entity.CreatedAtUtc, entity.UpdatedAtUtc);
    }

    private static TransportCommandDto ToDto(TransportCommand entity)
    {
        return new TransportCommandDto(entity.CommandId, entity.ManufacturerId, entity.CommandName, entity.ProcessType, entity.Description, entity.SortOrder, entity.CreatedAtUtc, entity.UpdatedAtUtc);
    }
}
