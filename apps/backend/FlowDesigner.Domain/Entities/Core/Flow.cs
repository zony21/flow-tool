namespace FlowDesigner.Domain.Entities.Core;

public class Flow
{
    public Guid FlowId { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<Lane> Lanes { get; set; } = new List<Lane>();
    public ICollection<Stage> Stages { get; set; } = new List<Stage>();
    public ICollection<FlowNode> Nodes { get; set; } = new List<FlowNode>();
    public ICollection<FlowLink> Links { get; set; } = new List<FlowLink>();
    public ICollection<FlowComment> Comments { get; set; } = new List<FlowComment>();
    public ICollection<FlowVersion> Versions { get; set; } = new List<FlowVersion>();
    public ICollection<FlowMetadata> MetadataItems { get; set; } = new List<FlowMetadata>();
}
