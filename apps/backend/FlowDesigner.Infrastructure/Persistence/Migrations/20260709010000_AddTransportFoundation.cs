using System;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260709010000_AddTransportFoundation")]
    public partial class AddTransportFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlowType",
                table: "FLOW",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "NORMAL");

            migrationBuilder.AddColumn<string>(
                name: "StageType",
                table: "STAGE",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "AUTO");

            migrationBuilder.CreateTable(
                name: "TRANSPORT_MANUFACTURER",
                columns: table => new
                {
                    ManufacturerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSPORT_MANUFACTURER", x => x.ManufacturerId);
                });

            migrationBuilder.CreateTable(
                name: "TRANSPORT_LOCATION",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    LocationType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSPORT_LOCATION", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_TRANSPORT_LOCATION_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRANSPORT_EQUIPMENT",
                columns: table => new
                {
                    EquipmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSPORT_EQUIPMENT", x => x.EquipmentId);
                    table.ForeignKey(
                        name: "FK_TRANSPORT_EQUIPMENT_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "PROJECT",
                        principalColumn: "PROJECT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRANSPORT_COMMAND",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommandName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ProcessType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSPORT_COMMAND", x => x.CommandId);
                    table.ForeignKey(
                        name: "FK_TRANSPORT_COMMAND_TRANSPORT_MANUFACTURER_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "TRANSPORT_MANUFACTURER",
                        principalColumn: "ManufacturerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FLOW_ProjectId_FlowType",
                table: "FLOW",
                columns: new[] { "ProjectId", "FlowType" });

            migrationBuilder.CreateIndex(
                name: "IX_TRANSPORT_MANUFACTURER_Name_IsDeleted",
                table: "TRANSPORT_MANUFACTURER",
                columns: new[] { "Name", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TRANSPORT_COMMAND_ManufacturerId_CommandName_IsDeleted",
                table: "TRANSPORT_COMMAND",
                columns: new[] { "ManufacturerId", "CommandName", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TRANSPORT_LOCATION_ProjectId_Name_IsDeleted",
                table: "TRANSPORT_LOCATION",
                columns: new[] { "ProjectId", "Name", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TRANSPORT_EQUIPMENT_ProjectId_Name_IsDeleted",
                table: "TRANSPORT_EQUIPMENT",
                columns: new[] { "ProjectId", "Name", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TRANSPORT_COMMAND");
            migrationBuilder.DropTable(name: "TRANSPORT_LOCATION");
            migrationBuilder.DropTable(name: "TRANSPORT_EQUIPMENT");
            migrationBuilder.DropTable(name: "TRANSPORT_MANUFACTURER");

            migrationBuilder.DropIndex(
                name: "IX_FLOW_ProjectId_FlowType",
                table: "FLOW");

            migrationBuilder.DropColumn(
                name: "StageType",
                table: "STAGE");

            migrationBuilder.DropColumn(
                name: "FlowType",
                table: "FLOW");
        }
    }
}
