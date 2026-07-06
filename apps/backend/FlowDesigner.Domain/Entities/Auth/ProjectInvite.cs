using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Auth;

public class ProjectInvite
{
    public Guid ProjectInviteId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public Project Project { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
}
