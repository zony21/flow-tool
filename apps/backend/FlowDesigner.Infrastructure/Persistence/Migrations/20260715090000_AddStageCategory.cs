using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowDesigner.Infrastructure.Persistence.Migrations;

public partial class AddStageCategory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "STAGE",
            type: "TEXT",
            maxLength: 40,
            nullable: false,
            defaultValue: "EQUIPMENT");

        migrationBuilder.Sql("UPDATE STAGE SET Category = 'EQUIPMENT' WHERE Category IS NULL OR TRIM(Category) = '';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Category", table: "STAGE");
    }
}
