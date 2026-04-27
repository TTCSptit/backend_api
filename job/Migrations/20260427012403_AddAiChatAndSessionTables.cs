using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace job.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChatAndSessionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AiDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiChatSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    User1Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    User2Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessages_SessionId",
                table: "AiChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AiChatMessages_UserId",
                table: "AiChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiChatSessions_UserId",
                table: "AiChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_User1Id",
                table: "ChatSessions",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_User2Id",
                table: "ChatSessions",
                column: "User2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiChatMessages");

            migrationBuilder.DropTable(
                name: "AiChatSessions");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "ChatMessages");
        }
    }
}
