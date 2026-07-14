namespace FlowDesigner.Domain.Entities.Transport;
public class TransportVehicleModel
{
    public Guid VehicleModelId { get; set; }
    public Guid ManufacturerId { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
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
    public TransportManufacturer Manufacturer { get; set; } = null!;
    public ICollection<FlowDesigner.Domain.Entities.Core.FlowNode> Nodes { get; set; } = new List<FlowDesigner.Domain.Entities.Core.FlowNode>();
}
