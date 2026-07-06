using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Settings;

public class AiSetting
{
    public Guid AiSettingId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public string ModelName { get; set; } = "gpt-5.3-codex";
    public bool AutoReviewEnabled { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
