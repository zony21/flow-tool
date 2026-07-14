namespace FlowDesigner.Domain.Entities.Transport;

public class TransportCommand
{
    public Guid CommandId { get; set; }
    public Guid ManufacturerId { get; set; }
    public Guid? ManufacturerVehicleTypeId { get; set; }
    public string CommandCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CommandName { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }

    public TransportManufacturer Manufacturer { get; set; } = null!;
    public TransportManufacturerVehicleType? ManufacturerVehicleType { get; set; }
}
