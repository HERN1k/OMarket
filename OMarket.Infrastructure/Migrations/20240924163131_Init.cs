using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminsCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Hash = table.Column<string>(type: "char(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminsCredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminsPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Permission = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminsPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

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
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeName = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAddresses", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_AdminsCredentials_CredentialsId",
                        column: x => x.CredentialsId,
                        principalTable: "AdminsCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admins_AdminsPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "AdminsPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnderTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnderTypeName = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnderTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductUnderTypes_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreTelegramChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_StoreAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "StoreAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_StoreTelegramChats_StoreTelegramChatId",
                        column: x => x.StoreTelegramChatId,
                        principalTable: "StoreTelegramChats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    PhotoUri = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    TypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnderTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Dimensions = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    Description = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductBrands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "ProductBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductUnderTypes_UnderTypeId",
                        column: x => x.UnderTypeId,
                        principalTable: "ProductUnderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    FirstName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true),
                    IsBot = table.Column<bool>(type: "boolean", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataStoreProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductUnderTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStoreProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataStoreProducts_ProductUnderTypes_ProductUnderTypeId",
                        column: x => x.ProductUnderTypeId,
                        principalTable: "ProductUnderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataStoreProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataStoreProducts_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_OrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_CredentialsId",
                table: "Admins",
                column: "CredentialsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Id",
                table: "Admins",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_PermissionId",
                table: "Admins",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminsCredentials_Id",
                table: "AdminsCredentials",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminsPermissions_Id",
                table: "AdminsPermissions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName",
                table: "Cities",
                column: "CityName");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Id",
                table: "Cities",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Id",
                table: "Customers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StoreId",
                table: "Customers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_Id",
                table: "DataStoreProducts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_ProductId",
                table: "DataStoreProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_ProductUnderTypeId",
                table: "DataStoreProducts",
                column: "ProductUnderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_Status",
                table: "DataStoreProducts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DataStoreProducts_StoreId",
                table: "DataStoreProducts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Id",
                table: "OrderItems",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Id",
                table: "Orders",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StatusId",
                table: "Orders",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatuses_Id",
                table: "OrderStatuses",
                column: "Id",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Id",
                table: "Products",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TypeId",
                table: "Products",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnderTypeId",
                table: "Products",
                column: "UnderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_Id",
                table: "ProductTypes",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_TypeName",
                table: "ProductTypes",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnderTypes_Id",
                table: "ProductUnderTypes",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnderTypes_ProductTypeId",
                table: "ProductUnderTypes",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnderTypes_UnderTypeName",
                table: "ProductUnderTypes",
                column: "UnderTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAddresses_Id",
                table: "StoreAddresses",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_AddressId",
                table: "Stores",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_AdminId",
                table: "Stores",
                column: "AdminId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Id",
                table: "Stores",
                column: "Id",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataStoreProducts");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductBrandProductUnderType");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "ProductBrands");

            migrationBuilder.DropTable(
                name: "ProductUnderTypes");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "StoreAddresses");

            migrationBuilder.DropTable(
                name: "StoreTelegramChats");

            migrationBuilder.DropTable(
                name: "AdminsCredentials");

            migrationBuilder.DropTable(
                name: "AdminsPermissions");
        }
    }
}
