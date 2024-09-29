using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropTableBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductBrands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductBrandProductUnderType");

            migrationBuilder.DropTable(
                name: "ProductBrands");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProductBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandName = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductBrandProductUnderType",
                columns: table => new
                {
                    ProductBrandsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductUnderTypesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBrandProductUnderType", x => new { x.ProductBrandsId, x.ProductUnderTypesId });
                    table.ForeignKey(
                        name: "FK_ProductBrandProductUnderType_ProductBrands_ProductBrandsId",
                        column: x => x.ProductBrandsId,
                        principalTable: "ProductBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBrandProductUnderType_ProductUnderTypes_ProductUnder~",
                        column: x => x.ProductUnderTypesId,
                        principalTable: "ProductUnderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrandProductUnderType_ProductUnderTypesId",
                table: "ProductBrandProductUnderType",
                column: "ProductUnderTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrands_BrandName",
                table: "ProductBrands",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrands_Id",
                table: "ProductBrands",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductBrands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
