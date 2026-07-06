namespace FlowDesigner.Domain.Entities.Core;

public class FlowComment
{
    public Guid CommentId { get; set; }
    public Guid FlowId { get; set; }
    public Guid? NodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }

    public Flow Flow { get; set; } = null!;
    public FlowNode? Node { get; set; }
}
