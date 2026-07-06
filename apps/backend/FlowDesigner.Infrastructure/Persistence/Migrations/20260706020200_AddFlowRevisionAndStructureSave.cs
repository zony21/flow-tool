using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowRevisionAndStructureSave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STAGE_LANE_LaneId",
                table: "STAGE");

            migrationBuilder.RenameColumn(
                name: "LaneId",
                table: "STAGE",
                newName: "FlowId");

            migrationBuilder.RenameIndex(
                name: "IX_STAGE_LaneId_SortOrder",
                table: "STAGE",
                newName: "IX_STAGE_FlowId_SortOrder");

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "FLOW",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_STAGE_FLOW_FlowId",
                table: "STAGE",
                column: "FlowId",
                principalTable: "FLOW",
                principalColumn: "FlowId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_STAGE_FLOW_FlowId",
                table: "STAGE");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "FLOW");

            migrationBuilder.RenameColumn(
                name: "FlowId",
                table: "STAGE",
                newName: "LaneId");

            migrationBuilder.RenameIndex(
                name: "IX_STAGE_FlowId_SortOrder",
                table: "STAGE",
                newName: "IX_STAGE_LaneId_SortOrder");

            migrationBuilder.AddForeignKey(
                name: "FK_STAGE_LANE_LaneId",
                table: "STAGE",
                column: "LaneId",
                principalTable: "LANE",
                principalColumn: "LaneId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
