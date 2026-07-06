using FlowDesigner.Domain.Entities.Auth;

namespace FlowDesigner.Domain.Entities.Settings;

public class UserSetting
{
    public Guid UserSettingId { get; set; }
    public Guid UserId { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "ja";

    public User User { get; set; } = null!;
}
