using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PERMISSION",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionCode = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSION", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "PROJECT",
                columns: table => new
                {
                    PROJECT_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    NAME = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT", x => x.PROJECT_ID);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleCode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNT",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GitHubId = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACCOUNT", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "FLOW",
                columns: table => new
                {
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FLOW", x => x.FlowId);
                    table.ForeignKey(
                        name: "FK_FLOW_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROJECT_SETTING",
                columns: table => new
                {
                    ProjectSettingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutoSaveEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSaveIntervalSec = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT_SETTING", x => x.ProjectSettingId);
                    table.ForeignKey(
                        name: "FK_PROJECT_SETTING_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ROLE_PERMISSION",
                columns: table => new
                {
                    RolePermissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermissionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE_PERMISSION", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSION_PERMISSION_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "PERMISSION",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSION_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ROLE",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AI_SETTING",
                columns: table => new
                {
                    AiSettingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    AutoReviewEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AI_SETTING", x => x.AiSettingId);
                    table.ForeignKey(
                        name: "FK_AI_SETTING_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AI_SETTING_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AUDIT_LOG",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Detail = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIT_LOG", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AUDIT_LOG_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AUDIT_LOG_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EDITOR_SETTING",
                columns: table => new
                {
                    EditorSettingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GridEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SnapEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EDITOR_SETTING", x => x.EditorSettingId);
                    table.ForeignKey(
                        name: "FK_EDITOR_SETTING_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EDITOR_SETTING_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EXPORT_SETTING",
                columns: table => new
                {
                    ExportSettingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefaultFormat = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    IncludeMetadata = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXPORT_SETTING", x => x.ExportSettingId);
                    table.ForeignKey(
                        name: "FK_EXPORT_SETTING_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EXPORT_SETTING_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROJECT_INVITE",
                columns: table => new
                {
                    ProjectInviteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT_INVITE", x => x.ProjectInviteId);
                    table.ForeignKey(
                        name: "FK_PROJECT_INVITE_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROJECT_INVITE_USER_ACCOUNT_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PROJECT_MEMBER",
                columns: table => new
                {
                    ProjectMemberId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROJECT_MEMBER", x => x.ProjectMemberId);
                    table.ForeignKey(
                        name: "FK_PROJECT_MEMBER_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PROJECT_MEMBER_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ROLE",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PROJECT_MEMBER_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_SETTING",
                columns: table => new
                {
                    UserSettingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_SETTING", x => x.UserSettingId);
                    table.ForeignKey(
                        name: "FK_USER_SETTING_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IMAGE",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IMAGE", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_IMAGE_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IMAGE_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LANE",
                columns: table => new
                {
                    LaneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LANE", x => x.LaneId);
                    table.ForeignKey(
                        name: "FK_LANE_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "METADATA",
                columns: table => new
                {
                    MetadataId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetaKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    MetaValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_METADATA", x => x.MetadataId);
                    table.ForeignKey(
                        name: "FK_METADATA_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VERSION",
                columns: table => new
                {
                    VersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VERSION", x => x.VersionId);
                    table.ForeignKey(
                        name: "FK_VERSION_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STAGE",
                columns: table => new
                {
                    StageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LaneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STAGE", x => x.StageId);
                    table.ForeignKey(
                        name: "FK_STAGE_LANE_LaneId",
                        column: x => x.LaneId,
                        principalTable: "LANE",
                        principalColumn: "LaneId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NODE",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LaneId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NodeType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NODE", x => x.NodeId);
                    table.ForeignKey(
                        name: "FK_NODE_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NODE_LANE_LaneId",
                        column: x => x.LaneId,
                        principalTable: "LANE",
                        principalColumn: "LaneId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NODE_STAGE_StageId",
                        column: x => x.StageId,
                        principalTable: "STAGE",
                        principalColumn: "StageId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "COMMENT",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NodeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMMENT", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_COMMENT_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COMMENT_NODE_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NODE",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LINK",
                columns: table => new
                {
                    LinkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FlowId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Condition = table.Column<string>(type: "TEXT", nullable: true),
                    Label = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LINK", x => x.LinkId);
                    table.ForeignKey(
                        name: "FK_LINK_FLOW_FlowId",
                        column: x => x.FlowId,
                        principalTable: "FLOW",
                        principalColumn: "FlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LINK_NODE_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "NODE",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LINK_NODE_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "NODE",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AI_SETTING_ProjectId",
                table: "AI_SETTING",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AI_SETTING_UserId_ProjectId",
                table: "AI_SETTING",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_ProjectId",
                table: "AUDIT_LOG",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_UserId",
                table: "AUDIT_LOG",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENT_FlowId",
                table: "COMMENT",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENT_NodeId",
                table: "COMMENT",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EDITOR_SETTING_ProjectId",
                table: "EDITOR_SETTING",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EDITOR_SETTING_UserId_ProjectId",
                table: "EDITOR_SETTING",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EXPORT_SETTING_ProjectId",
                table: "EXPORT_SETTING",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EXPORT_SETTING_UserId_ProjectId",
                table: "EXPORT_SETTING",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FLOW_ProjectId_Name",
                table: "FLOW",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_FlowId",
                table: "IMAGE",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_IMAGE_ProjectId",
                table: "IMAGE",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LANE_FlowId_SortOrder",
                table: "LANE",
                columns: new[] { "FlowId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LINK_FlowId",
                table: "LINK",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_LINK_SourceNodeId",
                table: "LINK",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_LINK_TargetNodeId",
                table: "LINK",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_METADATA_FlowId_MetaKey",
                table: "METADATA",
                columns: new[] { "FlowId", "MetaKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NODE_FlowId",
                table: "NODE",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_LaneId",
                table: "NODE",
                column: "LaneId");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_StageId",
                table: "NODE",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSION_PermissionCode",
                table: "PERMISSION",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_INVITE_InvitedByUserId",
                table: "PROJECT_INVITE",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_INVITE_ProjectId_Email_Status",
                table: "PROJECT_INVITE",
                columns: new[] { "ProjectId", "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_MEMBER_ProjectId_UserId",
                table: "PROJECT_MEMBER",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_MEMBER_RoleId",
                table: "PROJECT_MEMBER",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_MEMBER_UserId",
                table: "PROJECT_MEMBER",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECT_SETTING_ProjectId",
                table: "PROJECT_SETTING",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_RoleCode",
                table: "ROLE",
                column: "RoleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSION_PermissionId",
                table: "ROLE_PERMISSION",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSION_RoleId_PermissionId",
                table: "ROLE_PERMISSION",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_STAGE_LaneId_SortOrder",
                table: "STAGE",
                columns: new[] { "LaneId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_GitHubId",
                table: "USER_ACCOUNT",
                column: "GitHubId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_SETTING_UserId",
                table: "USER_SETTING",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VERSION_FlowId_VersionNumber",
                table: "VERSION",
                columns: new[] { "FlowId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AI_SETTING");

            migrationBuilder.DropTable(
                name: "AUDIT_LOG");

            migrationBuilder.DropTable(
                name: "COMMENT");

            migrationBuilder.DropTable(
                name: "EDITOR_SETTING");

            migrationBuilder.DropTable(
                name: "EXPORT_SETTING");

            migrationBuilder.DropTable(
                name: "IMAGE");

            migrationBuilder.DropTable(
                name: "LINK");

            migrationBuilder.DropTable(
                name: "METADATA");

            migrationBuilder.DropTable(
                name: "PROJECT_INVITE");

            migrationBuilder.DropTable(
                name: "PROJECT_MEMBER");

            migrationBuilder.DropTable(
                name: "PROJECT_SETTING");

            migrationBuilder.DropTable(
                name: "ROLE_PERMISSION");

            migrationBuilder.DropTable(
                name: "USER_SETTING");

            migrationBuilder.DropTable(
                name: "VERSION");

            migrationBuilder.DropTable(
                name: "NODE");

            migrationBuilder.DropTable(
                name: "PERMISSION");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.DropTable(
                name: "USER_ACCOUNT");

            migrationBuilder.DropTable(
                name: "STAGE");

            migrationBuilder.DropTable(
                name: "LANE");

            migrationBuilder.DropTable(
                name: "FLOW");

            migrationBuilder.DropTable(
                name: "PROJECT");
        }
    }
}
