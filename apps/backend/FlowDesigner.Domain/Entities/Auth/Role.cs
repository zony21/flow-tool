namespace FlowDesigner.Domain.Entities.Auth;

public class Role
{
    public Guid RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
