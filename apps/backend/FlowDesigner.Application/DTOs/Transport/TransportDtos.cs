namespace FlowDesigner.Application.DTOs.Transport;

public sealed record TransportManufacturerDto(
    Guid ManufacturerId,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportManufacturerVehicleTypeDto(
    Guid ManufacturerVehicleTypeId,
    Guid ManufacturerId,
    string ManufacturerName,
    string VehicleType,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportCommandDto(
    Guid CommandId,
    Guid ManufacturerVehicleTypeId,
    Guid ManufacturerId,
    string ManufacturerName,
    string VehicleType,
    string CommandCode,
    string CommandName,
    string ProcessType,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportLocationDto(
    Guid LocationId,
    Guid ProjectId,
    string Name,
    string LocationType,
    string? Description,
    int SortOrder,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportEquipmentDto(
    Guid EquipmentId,
    Guid ProjectId,
    string Name,
    string Category,
    string? Description,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record SaveTransportManufacturerRequest(
    string Name,
    string? Description,
    int? SortOrder,
    bool IsActive = true);

public sealed record SaveTransportManufacturerVehicleTypeRequest(
    string VehicleType,
    string? Description,
    int? SortOrder,
    bool IsActive = true);

public sealed record SaveTransportCommandRequest(
    string CommandCode,
    string CommandName,
    string ProcessType,
    string? Description,
    int? SortOrder,
    bool IsActive = true);

public sealed record SaveTransportLocationRequest(
    string Name,
    string LocationType,
    string? Description,
    int? SortOrder);

public sealed record SaveTransportEquipmentRequest(
    string Name,
    string Category,
    string? Description,
    int? SortOrder);

