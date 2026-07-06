namespace FlowDesigner.Domain.Entities.Auth;

public class User
{
    public Guid UserId { get; set; }
    public string GitHubId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectInvite> ProjectInvites { get; set; } = new List<ProjectInvite>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
