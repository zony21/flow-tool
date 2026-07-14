using FlowDesigner.Domain.Entities.Transport;

namespace FlowDesigner.Domain.Entities.Core;

public class FlowNode
{
    public Guid NodeId { get; set; }
    public Guid FlowId { get; set; }
    public Guid? LaneId { get; set; }
    public Guid? StageId { get; set; }
    public string NodeType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CommandId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? EquipmentId { get; set; }
    public Guid? VehicleModelId { get; set; }
    public string? RwType { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public Flow Flow { get; set; } = null!;
    public Lane? Lane { get; set; }
    public Stage? Stage { get; set; }
    public TransportCommand? Command { get; set; }
    public TransportLocation? Location { get; set; }
    public TransportEquipment? Equipment { get; set; }
    public TransportVehicleModel? VehicleModel { get; set; }
    public ICollection<FlowLink> OutgoingLinks { get; set; } = new List<FlowLink>();
    public ICollection<FlowLink> IncomingLinks { get; set; } = new List<FlowLink>();
}
