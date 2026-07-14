using FlowDesigner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260714020000_AddManufacturerVehicleTypeHierarchy")]
public partial class AddManufacturerVehicleTypeHierarchy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "TRANSPORT_MANUFACTURER",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateTable(
            name: "TRANSPORT_MANUFACTURER_VEHICLE_TYPE",
            columns: table => new
            {
                ManufacturerVehicleTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                ManufacturerId = table.Column<Guid>(type: "TEXT", nullable: false),
                VehicleType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
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
                table.PrimaryKey("PK_TRANSPORT_MANUFACTURER_VEHICLE_TYPE", x => x.ManufacturerVehicleTypeId);
                table.ForeignKey(
                    name: "FK_TRANSPORT_MANUFACTURER_VEHICLE_TYPE_TRANSPORT_MANUFACTURER_ManufacturerId",
                    column: x => x.ManufacturerId,
                    principalTable: "TRANSPORT_MANUFACTURER",
                    principalColumn: "ManufacturerId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.AddColumn<Guid>(name: "ManufacturerVehicleTypeId", table: "TRANSPORT_COMMAND", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "CommandCode", table: "TRANSPORT_COMMAND", type: "TEXT", maxLength: 120, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<bool>(name: "IsActive", table: "TRANSPORT_COMMAND", type: "INTEGER", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<Guid>(name: "ManufacturerVehicleTypeId", table: "NODE", type: "TEXT", nullable: true);

        migrationBuilder.Sql("""
            INSERT INTO TRANSPORT_MANUFACTURER_VEHICLE_TYPE
                (ManufacturerVehicleTypeId, ManufacturerId, VehicleType, Description, SortOrder, IsActive, IsDeleted, CreatedAtUtc, CreatedByUserId, UpdatedAtUtc, UpdatedByUserId)
            SELECT
                lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' ||
                substr('89ab', abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6))),
                ManufacturerId,
                CASE WHEN upper(trim(VehicleType)) = 'AGV' THEN 'AGV' ELSE 'AGF' END,
                Description,
                SortOrder,
                1,
                IsDeleted,
                CreatedAtUtc,
                CreatedByUserId,
                UpdatedAtUtc,
                UpdatedByUserId
            FROM TRANSPORT_MANUFACTURER;
            """);

        migrationBuilder.Sql("""
            UPDATE TRANSPORT_COMMAND
            SET ManufacturerVehicleTypeId = (
                    SELECT mvt.ManufacturerVehicleTypeId
                    FROM TRANSPORT_MANUFACTURER_VEHICLE_TYPE mvt
                    WHERE mvt.ManufacturerId = TRANSPORT_COMMAND.ManufacturerId
                    LIMIT 1),
                CommandCode = upper(replace(trim(CommandName), ' ', '_'));
            """);

        migrationBuilder.CreateIndex(
            name: "IX_TRANSPORT_MANUFACTURER_VEHICLE_TYPE_ManufacturerId_VehicleType_IsDeleted",
            table: "TRANSPORT_MANUFACTURER_VEHICLE_TYPE",
            columns: new[] { "ManufacturerId", "VehicleType", "IsDeleted" },
            unique: true);
        migrationBuilder.CreateIndex(name: "IX_TRANSPORT_COMMAND_ManufacturerVehicleTypeId", table: "TRANSPORT_COMMAND", column: "ManufacturerVehicleTypeId");
        migrationBuilder.CreateIndex(name: "IX_NODE_ManufacturerVehicleTypeId", table: "NODE", column: "ManufacturerVehicleTypeId");

        // SQLite cannot add foreign keys to the existing TRANSPORT_COMMAND and NODE tables.
        // Their references are validated by the application save and master APIs.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_NODE_ManufacturerVehicleTypeId", "NODE");
        migrationBuilder.DropIndex("IX_TRANSPORT_COMMAND_ManufacturerVehicleTypeId", "TRANSPORT_COMMAND");
        migrationBuilder.DropColumn("ManufacturerVehicleTypeId", "NODE");
        migrationBuilder.DropColumn("ManufacturerVehicleTypeId", "TRANSPORT_COMMAND");
        migrationBuilder.DropColumn("CommandCode", "TRANSPORT_COMMAND");
        migrationBuilder.DropColumn("IsActive", "TRANSPORT_COMMAND");
        migrationBuilder.DropTable("TRANSPORT_MANUFACTURER_VEHICLE_TYPE");
        migrationBuilder.DropColumn("IsActive", "TRANSPORT_MANUFACTURER");
    }
}
