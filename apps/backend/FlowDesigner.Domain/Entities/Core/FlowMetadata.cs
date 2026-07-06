namespace FlowDesigner.Domain.Entities.Core;

public class FlowMetadata
{
    public Guid MetadataId { get; set; }
    public Guid FlowId { get; set; }
    public string MetaKey { get; set; } = string.Empty;
    public string MetaValue { get; set; } = string.Empty;

    public Flow Flow { get; set; } = null!;
}
