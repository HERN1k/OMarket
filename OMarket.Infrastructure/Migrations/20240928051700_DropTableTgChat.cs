using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropTableTgChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_StoreTelegramChats_StoreTelegramChatId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "StoreTelegramChats");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreTelegramChatId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StoreTelegramChatId",
                table: "Stores");

            migrationBuilder.AddColumn<long>(
                name: "TgChatId",
                table: "Stores",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TgChatId",
                table: "Stores");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreTelegramChatId",
                table: "Stores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "StoreTelegramChats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreTelegramChats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreTelegramChatId",
                table: "Stores",
                column: "StoreTelegramChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoreTelegramChats_ChatId",
                table: "StoreTelegramChats",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreTelegramChats_Id",
                table: "StoreTelegramChats",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_StoreTelegramChats_StoreTelegramChatId",
                table: "Stores",
                column: "StoreTelegramChatId",
                principalTable: "StoreTelegramChats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
