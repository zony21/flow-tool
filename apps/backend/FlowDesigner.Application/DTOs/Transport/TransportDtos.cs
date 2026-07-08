namespace FlowDesigner.Application.DTOs.Transport;

public sealed record TransportManufacturerDto(
    Guid ManufacturerId,
    string Name,
    string VehicleType,
    string? Description,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportCommandDto(
    Guid CommandId,
    Guid ManufacturerId,
    string CommandName,
    string ProcessType,
    string? Description,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TransportLocationDto(
    Guid LocationId,
    Guid ProjectId,
    string Name,
    string LocationType,
    string? Description,
    int SortOrder,
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
    string VehicleType,
    string? Description,
    int? SortOrder);

public sealed record SaveTransportCommandRequest(
    Guid ManufacturerId,
    string CommandName,
    string ProcessType,
    string? Description,
    int? SortOrder);

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
