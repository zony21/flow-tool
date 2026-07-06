namespace FlowDesigner.Domain.Entities.Core;

public class FlowLink
{
    public Guid LinkId { get; set; }
    public Guid FlowId { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string? Condition { get; set; }
    public string? Label { get; set; }

    public Flow Flow { get; set; } = null!;
    public FlowNode SourceNode { get; set; } = null!;
    public FlowNode TargetNode { get; set; } = null!;
}
