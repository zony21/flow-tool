namespace FlowDesigner.Domain.Entities.Core;

public class FlowVersion
{
    public Guid VersionId { get; set; }
    public Guid FlowId { get; set; }
    public int VersionNumber { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Flow Flow { get; set; } = null!;
    public FlowDesigner.Domain.Entities.Auth.User? CreatedByUser { get; set; }
}
