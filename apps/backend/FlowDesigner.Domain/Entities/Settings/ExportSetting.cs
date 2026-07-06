using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Settings;

public class ExportSetting
{
    public Guid ExportSettingId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public string DefaultFormat { get; set; } = "mermaid";
    public bool IncludeMetadata { get; set; } = true;

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
