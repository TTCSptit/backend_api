using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace job.Migrations
{
    /// <inheritdoc />
    public partial class AddAIEvaluationToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIReasoning",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AIScore",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AIStrengths",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AIWeaknesses",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIReasoning",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AIScore",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AIStrengths",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AIWeaknesses",
                table: "Applications");
        }
    }
}
