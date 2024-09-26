using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Stores",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BlockedOrders",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BlockedReviews",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "varchar(512)", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CreatedAt",
                table: "Reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Id",
                table: "Reviews",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StoreId",
                table: "Reviews",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropColumn(
                name: "BlockedOrders",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BlockedReviews",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Stores",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
