using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Settings;

namespace FlowDesigner.Domain.Entities.Core;

public class Project
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Flow> Flows { get; set; } = new List<Flow>();
    public ICollection<FlowImage> Images { get; set; } = new List<FlowImage>();
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<ProjectInvite> ProjectInvites { get; set; } = new List<ProjectInvite>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<ProjectSetting> ProjectSettings { get; set; } = new List<ProjectSetting>();
    public ICollection<EditorSetting> EditorSettings { get; set; } = new List<EditorSetting>();
    public ICollection<AiSetting> AiSettings { get; set; } = new List<AiSetting>();
    public ICollection<ExportSetting> ExportSettings { get; set; } = new List<ExportSetting>();
}
