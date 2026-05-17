using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace job.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommunicationScore = table.Column<int>(type: "int", nullable: false),
                    TechnicalScore = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    FeedbackStrengths = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeedbackWeaknesses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranscriptSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewReports_RoomId",
                table: "InterviewReports",
                column: "RoomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewReports");
        }
    }
}
