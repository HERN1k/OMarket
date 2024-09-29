using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdminTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TgAccountId",
                table: "Admins",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TgAccountId",
                table: "Admins");
        }
    }
}
