using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowVersionCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "VERSION",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VERSION_CreatedByUserId",
                table: "VERSION",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VERSION_USER_ACCOUNT_CreatedByUserId",
                table: "VERSION",
                column: "CreatedByUserId",
                principalTable: "USER_ACCOUNT",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VERSION_USER_ACCOUNT_CreatedByUserId",
                table: "VERSION");

            migrationBuilder.DropIndex(
                name: "IX_VERSION_CreatedByUserId",
                table: "VERSION");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "VERSION");
        }
    }
}
