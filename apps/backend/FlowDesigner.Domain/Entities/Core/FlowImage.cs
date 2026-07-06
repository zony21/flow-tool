namespace FlowDesigner.Domain.Entities.Core;

public class FlowImage
{
    public Guid ImageId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? FlowId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAtUtc { get; set; }

    public Project Project { get; set; } = null!;
    public Flow? Flow { get; set; }
}
