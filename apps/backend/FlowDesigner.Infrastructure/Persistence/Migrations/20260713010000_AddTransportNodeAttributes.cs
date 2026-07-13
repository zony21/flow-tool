using System;
using FlowDesigner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260713010000_AddTransportNodeAttributes")]
    public partial class AddTransportNodeAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommandId",
                table: "NODE",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "NODE",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EquipmentId",
                table: "NODE",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RwType",
                table: "NODE",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NODE_CommandId",
                table: "NODE",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_LocationId",
                table: "NODE",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_EquipmentId",
                table: "NODE",
                column: "EquipmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_NODE_TRANSPORT_COMMAND_CommandId",
                table: "NODE",
                column: "CommandId",
                principalTable: "TRANSPORT_COMMAND",
                principalColumn: "CommandId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NODE_TRANSPORT_LOCATION_LocationId",
                table: "NODE",
                column: "LocationId",
                principalTable: "TRANSPORT_LOCATION",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NODE_TRANSPORT_EQUIPMENT_EquipmentId",
                table: "NODE",
                column: "EquipmentId",
                principalTable: "TRANSPORT_EQUIPMENT",
                principalColumn: "EquipmentId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NODE_TRANSPORT_COMMAND_CommandId",
                table: "NODE");

            migrationBuilder.DropForeignKey(
                name: "FK_NODE_TRANSPORT_LOCATION_LocationId",
                table: "NODE");

            migrationBuilder.DropForeignKey(
                name: "FK_NODE_TRANSPORT_EQUIPMENT_EquipmentId",
                table: "NODE");

            migrationBuilder.DropIndex(
                name: "IX_NODE_CommandId",
                table: "NODE");

            migrationBuilder.DropIndex(
                name: "IX_NODE_LocationId",
                table: "NODE");

            migrationBuilder.DropIndex(
                name: "IX_NODE_EquipmentId",
                table: "NODE");

            migrationBuilder.DropColumn(
                name: "CommandId",
                table: "NODE");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "NODE");

            migrationBuilder.DropColumn(
                name: "EquipmentId",
                table: "NODE");

            migrationBuilder.DropColumn(
                name: "RwType",
                table: "NODE");
        }
    }
}
