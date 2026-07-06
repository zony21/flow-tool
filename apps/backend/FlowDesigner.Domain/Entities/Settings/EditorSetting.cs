using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Settings;

public class EditorSetting
{
    public Guid EditorSettingId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public bool GridEnabled { get; set; } = true;
    public bool SnapEnabled { get; set; } = true;

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
