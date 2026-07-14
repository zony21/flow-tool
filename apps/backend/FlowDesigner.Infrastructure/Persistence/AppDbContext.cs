using FlowDesigner.Domain.Entities.Auth;
using FlowDesigner.Domain.Entities.Core;
using FlowDesigner.Domain.Entities.Settings;
using FlowDesigner.Domain.Entities.Transport;
using Microsoft.EntityFrameworkCore;

namespace FlowDesigner.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Flow> Flows => Set<Flow>();
    public DbSet<Lane> Lanes => Set<Lane>();
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<FlowNode> Nodes => Set<FlowNode>();
    public DbSet<FlowLink> Links => Set<FlowLink>();
    public DbSet<FlowComment> Comments => Set<FlowComment>();
    public DbSet<FlowImage> Images => Set<FlowImage>();
    public DbSet<FlowVersion> Versions => Set<FlowVersion>();
    public DbSet<FlowMetadata> MetadataItems => Set<FlowMetadata>();

    public DbSet<TransportManufacturer> TransportManufacturers => Set<TransportManufacturer>();
    public DbSet<TransportCommand> TransportCommands => Set<TransportCommand>();
    public DbSet<TransportLocation> TransportLocations => Set<TransportLocation>();
    public DbSet<TransportEquipment> TransportEquipments => Set<TransportEquipment>();
    public DbSet<TransportVehicleModel> TransportVehicleModels => Set<TransportVehicleModel>();

    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ProjectInvite> ProjectInvites => Set<ProjectInvite>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<ProjectSetting> ProjectSettings => Set<ProjectSetting>();
    public DbSet<EditorSetting> EditorSettings => Set<EditorSetting>();
    public DbSet<AiSetting> AiSettings => Set<AiSetting>();
    public DbSet<ExportSetting> ExportSettings => Set<ExportSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("PROJECT");
            entity.HasKey(x => x.ProjectId);
            entity.Property(x => x.ProjectId).HasColumnName("PROJECT_ID");
            entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(2000);
            entity.Property(x => x.CreatedAtUtc).HasColumnName("CREATED_AT_UTC").IsRequired();
        });

        modelBuilder.Entity<Flow>(entity =>
        {
            entity.ToTable("FLOW");
            entity.HasKey(x => x.FlowId);
            entity.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.FlowType });
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FlowType).HasMaxLength(40).HasDefaultValue(FlowTypes.Normal).IsRequired();
            entity.Property(x => x.Revision).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.Flows).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lane>(entity =>
        {
            entity.ToTable("LANE");
            entity.HasKey(x => x.LaneId);
            entity.HasIndex(x => new { x.FlowId, x.SortOrder }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasOne<Flow>().WithMany(x => x.Lanes).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Stage>(entity =>
        {
            entity.ToTable("STAGE");
            entity.HasKey(x => x.StageId);
            entity.HasIndex(x => new { x.FlowId, x.SortOrder }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasOne<Flow>().WithMany(x => x.Stages).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlowNode>(entity =>
        {
            entity.ToTable("NODE");
            entity.HasKey(x => x.NodeId);
            entity.Property(x => x.NodeType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Flow).WithMany(x => x.Nodes).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Lane).WithMany().HasForeignKey(x => x.LaneId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Stage).WithMany().HasForeignKey(x => x.StageId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FlowLink>(entity =>
        {
            entity.ToTable("LINK");
            entity.HasKey(x => x.LinkId);
            entity.HasOne(x => x.Flow).WithMany(x => x.Links).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SourceNode).WithMany(x => x.OutgoingLinks).HasForeignKey(x => x.SourceNodeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TargetNode).WithMany(x => x.IncomingLinks).HasForeignKey(x => x.TargetNodeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FlowComment>(entity =>
        {
            entity.ToTable("COMMENT");
            entity.HasKey(x => x.CommentId);
            entity.Property(x => x.Text).HasMaxLength(4000).IsRequired();
            entity.HasOne(x => x.Flow).WithMany(x => x.Comments).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Node).WithMany().HasForeignKey(x => x.NodeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FlowImage>(entity =>
        {
            entity.ToTable("IMAGE");
            entity.HasKey(x => x.ImageId);
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.Images).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Flow).WithMany().HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FlowVersion>(entity =>
        {
            entity.ToTable("VERSION");
            entity.HasKey(x => x.VersionId);
            entity.HasIndex(x => new { x.FlowId, x.VersionNumber }).IsUnique();
            entity.Property(x => x.SnapshotJson).IsRequired();
            entity.HasOne(x => x.Flow).WithMany(x => x.Versions).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedFlowVersions).HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FlowMetadata>(entity =>
        {
            entity.ToTable("METADATA");
            entity.HasKey(x => x.MetadataId);
            entity.HasIndex(x => new { x.FlowId, x.MetaKey }).IsUnique();
            entity.Property(x => x.MetaKey).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MetaValue).HasMaxLength(4000).IsRequired();
            entity.HasOne(x => x.Flow).WithMany(x => x.MetadataItems).HasForeignKey(x => x.FlowId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransportManufacturer>(entity =>
        {
            entity.ToTable("TRANSPORT_MANUFACTURER");
            entity.HasKey(x => x.ManufacturerId);
            entity.HasIndex(x => new { x.Name, x.IsDeleted });
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<TransportCommand>(entity =>
        {
            entity.ToTable("TRANSPORT_COMMAND");
            entity.HasKey(x => x.CommandId);
            entity.HasIndex(x => new { x.ManufacturerId, x.CommandName, x.IsDeleted });
            entity.Property(x => x.CommandName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ProcessType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasOne(x => x.Manufacturer).WithMany(x => x.Commands).HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TransportLocation>(entity =>
        {
            entity.ToTable("TRANSPORT_LOCATION");
            entity.HasKey(x => x.LocationId);
            entity.HasIndex(x => new { x.ProjectId, x.Name, x.IsDeleted });
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.LocationType).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasOne(x => x.Project).WithMany(x => x.TransportLocations).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransportEquipment>(entity =>
        {
            entity.ToTable("TRANSPORT_EQUIPMENT");
            entity.HasKey(x => x.EquipmentId);
            entity.HasIndex(x => new { x.ProjectId, x.Name, x.IsDeleted });
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasOne(x => x.Project).WithMany(x => x.TransportEquipments).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransportVehicleModel>(entity =>
        {
            entity.ToTable("TRANSPORT_VEHICLE_MODEL"); entity.HasKey(x => x.VehicleModelId);
            entity.HasIndex(x => new { x.ManufacturerId, x.VehicleType, x.ModelCode, x.IsDeleted }).IsUnique();
            entity.Property(x => x.VehicleType).HasMaxLength(10).IsRequired(); entity.Property(x => x.ModelCode).HasMaxLength(120).IsRequired(); entity.Property(x => x.ModelName).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Manufacturer).WithMany(x => x.VehicleModels).HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FlowNode>().HasOne(x => x.VehicleModel).WithMany(x => x.Nodes).HasForeignKey(x => x.VehicleModelId).OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USER_ACCOUNT");
            entity.HasKey(x => x.UserId);
            entity.HasIndex(x => x.GitHubId).IsUnique();
            entity.Property(x => x.GitHubId).HasMaxLength(80).IsRequired();
            entity.Property(x => x.UserName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("PROJECT_MEMBER");
            entity.HasKey(x => x.ProjectMemberId);
            entity.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Project).WithMany(x => x.ProjectMembers).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany(x => x.ProjectMembers).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role).WithMany(x => x.ProjectMembers).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("ROLE");
            entity.HasKey(x => x.RoleId);
            entity.HasIndex(x => x.RoleCode).IsUnique();
            entity.Property(x => x.RoleCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("PERMISSION");
            entity.HasKey(x => x.PermissionId);
            entity.HasIndex(x => x.PermissionCode).IsUnique();
            entity.Property(x => x.PermissionCode).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("ROLE_PERMISSION");
            entity.HasKey(x => x.RolePermissionId);
            entity.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectInvite>(entity =>
        {
            entity.ToTable("PROJECT_INVITE");
            entity.HasKey(x => x.ProjectInviteId);
            entity.HasIndex(x => new { x.ProjectId, x.Email, x.Status });
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.Project).WithMany(x => x.ProjectInvites).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.InvitedByUser).WithMany(x => x.ProjectInvites).HasForeignKey(x => x.InvitedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AUDIT_LOG");
            entity.HasKey(x => x.AuditLogId);
            entity.Property(x => x.Action).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(4000);
            entity.HasOne(x => x.Project).WithMany(x => x.AuditLogs).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.ToTable("USER_SETTING");
            entity.HasKey(x => x.UserSettingId);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.Property(x => x.Theme).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Language).HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectSetting>(entity =>
        {
            entity.ToTable("PROJECT_SETTING");
            entity.HasKey(x => x.ProjectSettingId);
            entity.HasIndex(x => x.ProjectId).IsUnique();
            entity.HasOne(x => x.Project).WithMany(x => x.ProjectSettings).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EditorSetting>(entity =>
        {
            entity.ToTable("EDITOR_SETTING");
            entity.HasKey(x => x.EditorSettingId);
            entity.HasIndex(x => new { x.UserId, x.ProjectId }).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Project).WithMany(x => x.EditorSettings).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiSetting>(entity =>
        {
            entity.ToTable("AI_SETTING");
            entity.HasKey(x => x.AiSettingId);
            entity.HasIndex(x => new { x.UserId, x.ProjectId }).IsUnique();
            entity.Property(x => x.ModelName).HasMaxLength(120).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Project).WithMany(x => x.AiSettings).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExportSetting>(entity =>
        {
            entity.ToTable("EXPORT_SETTING");
            entity.HasKey(x => x.ExportSettingId);
            entity.HasIndex(x => new { x.UserId, x.ProjectId }).IsUnique();
            entity.Property(x => x.DefaultFormat).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Project).WithMany(x => x.ExportSettings).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
