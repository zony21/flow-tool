using FlowDesigner.Domain.Entities.Core;

namespace FlowDesigner.Domain.Entities.Auth;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTime OccurredAtUtc { get; set; }

    public Project? Project { get; set; }
    public User? User { get; set; }
}
