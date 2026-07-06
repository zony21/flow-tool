namespace FlowDesigner.Domain.Entities.Core;

public class Stage
{
    public Guid StageId { get; set; }
    public Guid LaneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Lane Lane { get; set; } = null!;
    public ICollection<FlowNode> Nodes { get; set; } = new List<FlowNode>();
}
