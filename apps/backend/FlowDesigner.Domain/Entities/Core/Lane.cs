namespace FlowDesigner.Domain.Entities.Core;

public class Lane
{
    public Guid LaneId { get; set; }
    public Guid FlowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Flow Flow { get; set; } = null!;
    public ICollection<Stage> Stages { get; set; } = new List<Stage>();
    public ICollection<FlowNode> Nodes { get; set; } = new List<FlowNode>();
}
