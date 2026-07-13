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

            // SQLiteは既存テーブルへの外部キー追加を直接サポートしない。
            // Transport参照の整合性はFlow保存時のアプリケーション検証で保証する。
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
