using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Auth;

public class ProjectMember
{
    public Guid ProjectMemberId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime JoinedAtUtc { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
