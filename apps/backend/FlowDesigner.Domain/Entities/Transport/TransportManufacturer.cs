namespace FlowDesigner.Domain.Entities.Transport;

public class TransportManufacturer
{
    public Guid ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }

    public ICollection<TransportCommand> Commands { get; set; } = new List<TransportCommand>();
    public ICollection<TransportManufacturerVehicleType> VehicleTypes { get; set; } = new List<TransportManufacturerVehicleType>();
}
