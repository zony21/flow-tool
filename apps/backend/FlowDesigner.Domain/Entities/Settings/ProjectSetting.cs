using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Settings;

public class ProjectSetting
{
    public Guid ProjectSettingId { get; set; }
    public Guid ProjectId { get; set; }
    public bool AutoSaveEnabled { get; set; } = true;
    public int AutoSaveIntervalSec { get; set; } = 30;

    public Project Project { get; set; } = null!;
}
