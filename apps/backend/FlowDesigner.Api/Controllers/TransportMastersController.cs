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
public sealed class TransportMastersController(AppDbContext dbContext, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("manufacturers")]
    public async Task<ActionResult<IReadOnlyList<TransportManufacturerDto>>> ListManufacturers(CancellationToken ct) => Ok(await dbContext.TransportManufacturers.AsNoTracking()
        .Where(x => !x.IsDeleted).OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => ToManufacturerDto(x)).ToListAsync(ct));

    [HttpPost("manufacturers")]
    public async Task<ActionResult<TransportManufacturerDto>> CreateManufacturer(SaveTransportManufacturerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return Validation<TransportManufacturerDto>("Manufacturer name is required.");
        var name = request.Name.Trim();
        if (await dbContext.TransportManufacturers.AnyAsync(x => !x.IsDeleted && x.Name == name, ct)) return Validation<TransportManufacturerDto>("Manufacturer name already exists.");
        var now = DateTime.UtcNow; var user = currentUserService.GetCurrentUserId();
        var entity = new TransportManufacturer { ManufacturerId = Guid.NewGuid(), Name = name, VehicleType = "", Description = request.Description?.Trim(), SortOrder = request.SortOrder ?? await NextManufacturerSortOrder(ct), IsActive = request.IsActive, CreatedAtUtc = now, UpdatedAtUtc = now, CreatedByUserId = user, UpdatedByUserId = user };
        dbContext.TransportManufacturers.Add(entity); await dbContext.SaveChangesAsync(ct);
        return Ok(ToManufacturerDto(entity));
    }

    [HttpPut("manufacturers/{id:guid}")]
    public async Task<ActionResult<TransportManufacturerDto>> UpdateManufacturer(Guid id, SaveTransportManufacturerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return Validation<TransportManufacturerDto>("Manufacturer name is required.");
        var entity = await dbContext.TransportManufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == id && !x.IsDeleted, ct);
        if (entity is null) return ApiError.NotFound<TransportManufacturerDto>(this, "Manufacturer was not found.");
        var name = request.Name.Trim();
        if (await dbContext.TransportManufacturers.AnyAsync(x => !x.IsDeleted && x.ManufacturerId != id && x.Name == name, ct)) return Validation<TransportManufacturerDto>("Manufacturer name already exists.");
        entity.Name = name; entity.Description = request.Description?.Trim(); entity.SortOrder = request.SortOrder ?? entity.SortOrder; entity.IsActive = request.IsActive; Touch(entity);
        await dbContext.SaveChangesAsync(ct); return Ok(ToManufacturerDto(entity));
    }

    [HttpDelete("manufacturers/{id:guid}")]
    public async Task<IActionResult> DeleteManufacturer(Guid id, CancellationToken ct)
    {
        var entity = await dbContext.TransportManufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Manufacturer was not found."));
        if (await dbContext.TransportManufacturerVehicleTypes.AnyAsync(x => x.ManufacturerId == id && !x.IsDeleted, ct)) return Conflict(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, "Delete the manufacturer's vehicle types first."));
        Delete(entity); await dbContext.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("manufacturers/{manufacturerId:guid}/vehicle-types")]
    public async Task<ActionResult<IReadOnlyList<TransportManufacturerVehicleTypeDto>>> ListVehicleTypes(Guid manufacturerId, CancellationToken ct) => Ok(await VehicleTypeQuery()
        .Where(x => x.ManufacturerId == manufacturerId).OrderBy(x => x.SortOrder).ThenBy(x => x.VehicleType).Select(x => ToVehicleTypeDto(x)).ToListAsync(ct));

    [HttpGet("manufacturer-vehicle-types")]
    public async Task<ActionResult<IReadOnlyList<TransportManufacturerVehicleTypeDto>>> ListAllVehicleTypes([FromQuery] bool includeInactive = true, CancellationToken ct = default)
    {
        var query = VehicleTypeQuery(); if (!includeInactive) query = query.Where(x => x.IsActive && x.Manufacturer.IsActive);
        return Ok(await query.OrderBy(x => x.Manufacturer.SortOrder).ThenBy(x => x.SortOrder).Select(x => ToVehicleTypeDto(x)).ToListAsync(ct));
    }

    [HttpPost("manufacturers/{manufacturerId:guid}/vehicle-types")]
    public async Task<ActionResult<TransportManufacturerVehicleTypeDto>> CreateVehicleType(Guid manufacturerId, SaveTransportManufacturerVehicleTypeRequest request, CancellationToken ct)
    {
        var error = await ValidateVehicleType(manufacturerId, request.VehicleType, null, ct); if (error is not null) return Validation<TransportManufacturerVehicleTypeDto>(error);
        var now = DateTime.UtcNow; var user = currentUserService.GetCurrentUserId();
        var entity = new TransportManufacturerVehicleType { ManufacturerVehicleTypeId = Guid.NewGuid(), ManufacturerId = manufacturerId, VehicleType = NormalizeType(request.VehicleType), Description = request.Description?.Trim(), SortOrder = request.SortOrder ?? await NextVehicleTypeSortOrder(manufacturerId, ct), IsActive = request.IsActive, CreatedAtUtc = now, UpdatedAtUtc = now, CreatedByUserId = user, UpdatedByUserId = user };
        dbContext.TransportManufacturerVehicleTypes.Add(entity); await dbContext.SaveChangesAsync(ct);
        return Ok(await VehicleTypeQuery().Where(x => x.ManufacturerVehicleTypeId == entity.ManufacturerVehicleTypeId).Select(x => ToVehicleTypeDto(x)).SingleAsync(ct));
    }

    [HttpPut("manufacturer-vehicle-types/{id:guid}")]
    public async Task<ActionResult<TransportManufacturerVehicleTypeDto>> UpdateVehicleType(Guid id, SaveTransportManufacturerVehicleTypeRequest request, CancellationToken ct)
    {
        var entity = await dbContext.TransportManufacturerVehicleTypes.FirstOrDefaultAsync(x => x.ManufacturerVehicleTypeId == id && !x.IsDeleted, ct);
        if (entity is null) return ApiError.NotFound<TransportManufacturerVehicleTypeDto>(this, "Manufacturer vehicle type was not found.");
        var error = await ValidateVehicleType(entity.ManufacturerId, request.VehicleType, id, ct); if (error is not null) return Validation<TransportManufacturerVehicleTypeDto>(error);
        entity.VehicleType = NormalizeType(request.VehicleType); entity.Description = request.Description?.Trim(); entity.SortOrder = request.SortOrder ?? entity.SortOrder; entity.IsActive = request.IsActive; Touch(entity);
        await dbContext.SaveChangesAsync(ct); return Ok(await VehicleTypeQuery().Where(x => x.ManufacturerVehicleTypeId == id).Select(x => ToVehicleTypeDto(x)).SingleAsync(ct));
    }

    [HttpDelete("manufacturer-vehicle-types/{id:guid}")]
    public async Task<IActionResult> DeleteVehicleType(Guid id, CancellationToken ct)
    {
        var entity = await dbContext.TransportManufacturerVehicleTypes.FirstOrDefaultAsync(x => x.ManufacturerVehicleTypeId == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Manufacturer vehicle type was not found."));
        if (await dbContext.Nodes.AnyAsync(x => x.ManufacturerVehicleTypeId == id, ct)) return Conflict(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, "This manufacturer vehicle type is assigned to a Node."));
        if (await dbContext.TransportCommands.AnyAsync(x => x.ManufacturerVehicleTypeId == id && !x.IsDeleted, ct)) return Conflict(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, "Delete its commands first."));
        Delete(entity); await dbContext.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("manufacturer-vehicle-types/{typeId:guid}/commands")]
    public async Task<ActionResult<IReadOnlyList<TransportCommandDto>>> ListCommands(Guid typeId, CancellationToken ct) => Ok(await CommandQuery().Where(x => x.ManufacturerVehicleTypeId == typeId).OrderBy(x => x.SortOrder).ThenBy(x => x.CommandName).Select(x => ToCommandDto(x)).ToListAsync(ct));

    [HttpGet("commands")]
    public async Task<ActionResult<IReadOnlyList<TransportCommandDto>>> ListAllCommands([FromQuery] Guid? manufacturerVehicleTypeId, [FromQuery] bool includeInactive = true, CancellationToken ct = default)
    {
        var query = CommandQuery(); if (manufacturerVehicleTypeId.HasValue) query = query.Where(x => x.ManufacturerVehicleTypeId == manufacturerVehicleTypeId); if (!includeInactive) query = query.Where(x => x.IsActive);
        return Ok(await query.OrderBy(x => x.SortOrder).ThenBy(x => x.CommandName).Select(x => ToCommandDto(x)).ToListAsync(ct));
    }

    [HttpPost("manufacturer-vehicle-types/{typeId:guid}/commands")]
    public async Task<ActionResult<TransportCommandDto>> CreateCommand(Guid typeId, SaveTransportCommandRequest request, CancellationToken ct)
    {
        var error = await ValidateCommand(typeId, request, null, ct); if (error is not null) return Validation<TransportCommandDto>(error);
        var type = await dbContext.TransportManufacturerVehicleTypes.SingleAsync(x => x.ManufacturerVehicleTypeId == typeId, ct); var now = DateTime.UtcNow; var user = currentUserService.GetCurrentUserId();
        var entity = new TransportCommand { CommandId = Guid.NewGuid(), ManufacturerVehicleTypeId = typeId, ManufacturerId = type.ManufacturerId, CommandCode = NormalizeCode(request.CommandCode), CommandName = request.CommandName.Trim(), ProcessType = request.ProcessType.Trim(), Description = request.Description?.Trim(), SortOrder = request.SortOrder ?? await NextCommandSortOrder(typeId, ct), IsActive = request.IsActive, CreatedAtUtc = now, UpdatedAtUtc = now, CreatedByUserId = user, UpdatedByUserId = user };
        dbContext.TransportCommands.Add(entity); await dbContext.SaveChangesAsync(ct); return Ok(await CommandQuery().Where(x => x.CommandId == entity.CommandId).Select(x => ToCommandDto(x)).SingleAsync(ct));
    }

    [HttpPut("commands/{id:guid}")]
    public async Task<ActionResult<TransportCommandDto>> UpdateCommand(Guid id, SaveTransportCommandRequest request, CancellationToken ct)
    {
        var entity = await dbContext.TransportCommands.FirstOrDefaultAsync(x => x.CommandId == id && !x.IsDeleted, ct); if (entity?.ManufacturerVehicleTypeId is null) return ApiError.NotFound<TransportCommandDto>(this, "Command was not found.");
        var error = await ValidateCommand(entity.ManufacturerVehicleTypeId.Value, request, id, ct); if (error is not null) return Validation<TransportCommandDto>(error);
        entity.CommandCode = NormalizeCode(request.CommandCode); entity.CommandName = request.CommandName.Trim(); entity.ProcessType = request.ProcessType.Trim(); entity.Description = request.Description?.Trim(); entity.SortOrder = request.SortOrder ?? entity.SortOrder; entity.IsActive = request.IsActive; Touch(entity);
        await dbContext.SaveChangesAsync(ct); return Ok(await CommandQuery().Where(x => x.CommandId == id).Select(x => ToCommandDto(x)).SingleAsync(ct));
    }

    [HttpDelete("commands/{id:guid}")]
    public async Task<IActionResult> DeleteCommand(Guid id, CancellationToken ct)
    {
        var entity = await dbContext.TransportCommands.FirstOrDefaultAsync(x => x.CommandId == id && !x.IsDeleted, ct); if (entity is null) return NotFound(ApiError.Create(HttpContext, ApiErrorCodes.NotFound, "Command was not found."));
        if (await dbContext.Nodes.AnyAsync(x => x.CommandId == id, ct)) return Conflict(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, "Command is assigned to a Node."));
        Delete(entity); await dbContext.SaveChangesAsync(ct); return NoContent();
    }

    private IQueryable<TransportManufacturerVehicleType> VehicleTypeQuery() => dbContext.TransportManufacturerVehicleTypes.AsNoTracking().Include(x => x.Manufacturer).Where(x => !x.IsDeleted && !x.Manufacturer.IsDeleted);
    private IQueryable<TransportCommand> CommandQuery() => dbContext.TransportCommands.AsNoTracking().Include(x => x.ManufacturerVehicleType).ThenInclude(x => x!.Manufacturer).Where(x => !x.IsDeleted && x.ManufacturerVehicleTypeId != null && !x.ManufacturerVehicleType!.IsDeleted);
    private ActionResult<T> Validation<T>(string message) => UnprocessableEntity(ApiError.Create(HttpContext, ApiErrorCodes.ValidationError, message));
    private static string NormalizeType(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private async Task<string?> ValidateVehicleType(Guid manufacturerId, string? value, Guid? currentId, CancellationToken ct) { var type = value is null ? "" : NormalizeType(value); if (type is not ("AGF" or "AGV")) return "VehicleType must be AGF or AGV."; if (!await dbContext.TransportManufacturers.AnyAsync(x => x.ManufacturerId == manufacturerId && !x.IsDeleted, ct)) return "Manufacturer was not found."; if (await dbContext.TransportManufacturerVehicleTypes.AnyAsync(x => x.ManufacturerId == manufacturerId && x.VehicleType == type && !x.IsDeleted && x.ManufacturerVehicleTypeId != currentId, ct)) return "VehicleType already exists for this Manufacturer."; return null; }
    private async Task<string?> ValidateCommand(Guid typeId, SaveTransportCommandRequest r, Guid? currentId, CancellationToken ct) { if (string.IsNullOrWhiteSpace(r.CommandCode) || string.IsNullOrWhiteSpace(r.CommandName) || string.IsNullOrWhiteSpace(r.ProcessType)) return "CommandCode, CommandName and ProcessType are required."; if (!await dbContext.TransportManufacturerVehicleTypes.AnyAsync(x => x.ManufacturerVehicleTypeId == typeId && !x.IsDeleted, ct)) return "Manufacturer vehicle type was not found."; var code = NormalizeCode(r.CommandCode); return await dbContext.TransportCommands.AnyAsync(x => x.ManufacturerVehicleTypeId == typeId && x.CommandCode == code && !x.IsDeleted && x.CommandId != currentId, ct) ? "CommandCode already exists for this Manufacturer and VehicleType." : null; }
    private async Task<int> NextManufacturerSortOrder(CancellationToken ct) => (await dbContext.TransportManufacturers.Where(x => !x.IsDeleted).MaxAsync(x => (int?)x.SortOrder, ct) ?? 0) + 1;
    private async Task<int> NextVehicleTypeSortOrder(Guid id, CancellationToken ct) => (await dbContext.TransportManufacturerVehicleTypes.Where(x => x.ManufacturerId == id && !x.IsDeleted).MaxAsync(x => (int?)x.SortOrder, ct) ?? 0) + 1;
    private async Task<int> NextCommandSortOrder(Guid id, CancellationToken ct) => (await dbContext.TransportCommands.Where(x => x.ManufacturerVehicleTypeId == id && !x.IsDeleted).MaxAsync(x => (int?)x.SortOrder, ct) ?? 0) + 1;
    private void Touch(dynamic x) { x.UpdatedAtUtc = DateTime.UtcNow; x.UpdatedByUserId = currentUserService.GetCurrentUserId(); }
    private void Delete(dynamic x) { x.IsDeleted = true; x.DeletedAtUtc = DateTime.UtcNow; x.DeletedByUserId = currentUserService.GetCurrentUserId(); Touch(x); }
    private static TransportManufacturerDto ToManufacturerDto(TransportManufacturer x) => new(x.ManufacturerId, x.Name, x.Description, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static TransportManufacturerVehicleTypeDto ToVehicleTypeDto(TransportManufacturerVehicleType x) => new(x.ManufacturerVehicleTypeId, x.ManufacturerId, x.Manufacturer.Name, x.VehicleType, x.Description, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static TransportCommandDto ToCommandDto(TransportCommand x) => new(x.CommandId, x.ManufacturerVehicleTypeId!.Value, x.ManufacturerVehicleType!.ManufacturerId, x.ManufacturerVehicleType.Manufacturer.Name, x.ManufacturerVehicleType.VehicleType, x.CommandCode, x.CommandName, x.ProcessType, x.Description, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
}
