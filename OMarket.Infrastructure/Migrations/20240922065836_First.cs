using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class First : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "DataStoreProducts");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductUnderTypeId",
                table: "DataStoreProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreAddressId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_ProductUnderTypeId",
                table: "DataStoreProducts",
                column: "ProductUnderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StoreAddressId",
                table: "Customers",
                column: "StoreAddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_StoreAddresses_StoreAddressId",
                table: "Customers",
                column: "StoreAddressId",
                principalTable: "StoreAddresses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataStoreProducts_ProductUnderTypes_ProductUnderTypeId",
                table: "DataStoreProducts",
                column: "ProductUnderTypeId",
                principalTable: "ProductUnderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_StoreAddresses_StoreAddressId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_DataStoreProducts_ProductUnderTypes_ProductUnderTypeId",
                table: "DataStoreProducts");

            migrationBuilder.DropIndex(
                name: "IX_DataStoreProducts_ProductUnderTypeId",
                table: "DataStoreProducts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_StoreAddressId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductUnderTypeId",
                table: "DataStoreProducts");

            migrationBuilder.DropColumn(
                name: "StoreAddressId",
                table: "Customers");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "DataStoreProducts",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
