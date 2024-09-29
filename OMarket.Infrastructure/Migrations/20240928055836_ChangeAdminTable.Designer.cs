﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

#nullable disable

namespace OMarket.Infrastructure.Migrations
{
    [DbContext(typeof(AppDBContext))]
    [Migration("20240928055836_ChangeAdminTable")]
    partial class ChangeAdminTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OMarket.Domain.Entities.Admin", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CredentialsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uuid");

                    b.Property<long?>("TgAccountId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CredentialsId")
                        .IsUnique();

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("PermissionId");

                    b.ToTable("Admins", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.AdminsCredentials", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("char(64)");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 5);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("AdminsCredentials", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.AdminsPermission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Permission")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 3);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("AdminsPermissions", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CityName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MinLength", 2);

                    b.HasKey("Id");

                    b.HasIndex("CityName");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Cities", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Customer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool>("BlockedOrders")
                        .HasColumnType("boolean");

                    b.Property<bool>("BlockedReviews")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasAnnotation("MinLength", 1);

                    b.Property<bool>("IsBot")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 10);

                    b.Property<Guid?>("StoreId")
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 5);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("StoreId");

                    b.ToTable("Customers", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.DataStoreProduct", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductUnderTypeId")
                        .HasColumnType("uuid");

                    b.Property<bool>("Status")
                        .HasColumnType("boolean");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("ProductId");

                    b.HasIndex("ProductUnderTypeId");

                    b.HasIndex("Status");

                    b.HasIndex("StoreId");

                    b.ToTable("DataStoreProducts", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("CustomerId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("StatusId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("StatusId");

                    b.HasIndex("StoreId");

                    b.ToTable("Orders", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.OrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(10, 2)");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.OrderStatus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 3);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("OrderStatuses", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("BrandId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Dimensions")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 1);

                    b.Property<string>("PhotoUri")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)")
                        .HasAnnotation("MinLength", 1);

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<Guid>("TypeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UnderTypeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("BrandId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("Name");

                    b.HasIndex("TypeId");

                    b.HasIndex("UnderTypeId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductBrand", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BrandName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 1);

                    b.HasKey("Id");

                    b.HasIndex("BrandName");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("ProductBrands", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 1);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("TypeName");

                    b.ToTable("ProductTypes", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductUnderType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductTypeId")
                        .HasColumnType("uuid");

                    b.Property<string>("UnderTypeName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 1);

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("ProductTypeId");

                    b.HasIndex("UnderTypeName");

                    b.ToTable("ProductUnderTypes", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Review", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("CustomerId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("varchar(512)")
                        .HasAnnotation("MinLength", 512);

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CustomerId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("StoreId");

                    b.ToTable("Reviews", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Store", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AddressId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AdminId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CityId")
                        .HasColumnType("uuid");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)")
                        .HasAnnotation("MinLength", 10);

                    b.Property<long?>("TgChatId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("AddressId")
                        .IsUnique();

                    b.HasIndex("AdminId")
                        .IsUnique();

                    b.HasIndex("CityId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Stores", (string)null);
                });

            modelBuilder.Entity("OMarket.Domain.Entities.StoreAddress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasAnnotation("MinLength", 3);

                    b.Property<decimal>("Latitude")
                        .HasColumnType("decimal(9, 6)");

                    b.Property<decimal>("Longitude")
                        .HasColumnType("decimal(9, 6)");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("StoreAddresses", (string)null);
                });

            modelBuilder.Entity("ProductBrandProductUnderType", b =>
                {
                    b.Property<Guid>("ProductBrandsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductUnderTypesId")
                        .HasColumnType("uuid");

                    b.HasKey("ProductBrandsId", "ProductUnderTypesId");

                    b.HasIndex("ProductUnderTypesId");

                    b.ToTable("ProductBrandProductUnderType");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Admin", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.AdminsCredentials", "AdminsCredentials")
                        .WithOne("Admin")
                        .HasForeignKey("OMarket.Domain.Entities.Admin", "CredentialsId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.AdminsPermission", "AdminsPermission")
                        .WithMany("Admins")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AdminsCredentials");

                    b.Navigation("AdminsPermission");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Customer", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.Store", "Store")
                        .WithMany("Customers")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("Store");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.DataStoreProduct", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.Product", "Product")
                        .WithMany("DataStoreProducts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.ProductUnderType", "ProductUnderType")
                        .WithMany("DataStoreProducts")
                        .HasForeignKey("ProductUnderTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.Store", "Store")
                        .WithMany("DataStoreProducts")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("ProductUnderType");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Order", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.OrderStatus", "OrderStatus")
                        .WithMany("Orders")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.Store", "Store")
                        .WithMany("Orders")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("OrderStatus");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.OrderItem", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.Product", "Product")
                        .WithMany("OrderItems")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Product", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.ProductBrand", "ProductBrand")
                        .WithMany("Products")
                        .HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.ProductType", "ProductType")
                        .WithMany("Products")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.ProductUnderType", "ProductUnderType")
                        .WithMany("Products")
                        .HasForeignKey("UnderTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ProductBrand");

                    b.Navigation("ProductType");

                    b.Navigation("ProductUnderType");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductUnderType", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.ProductType", "ProductType")
                        .WithMany("ProductUnderTypes")
                        .HasForeignKey("ProductTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ProductType");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Review", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.Customer", "Customer")
                        .WithMany("Reviews")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.Store", "Store")
                        .WithMany("Reviews")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Store", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.StoreAddress", "Address")
                        .WithOne("Store")
                        .HasForeignKey("OMarket.Domain.Entities.Store", "AddressId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.Admin", "Admin")
                        .WithOne("Store")
                        .HasForeignKey("OMarket.Domain.Entities.Store", "AdminId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.City", "City")
                        .WithMany("Stores")
                        .HasForeignKey("CityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("Admin");

                    b.Navigation("City");
                });

            modelBuilder.Entity("ProductBrandProductUnderType", b =>
                {
                    b.HasOne("OMarket.Domain.Entities.ProductBrand", null)
                        .WithMany()
                        .HasForeignKey("ProductBrandsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMarket.Domain.Entities.ProductUnderType", null)
                        .WithMany()
                        .HasForeignKey("ProductUnderTypesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Admin", b =>
                {
                    b.Navigation("Store");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.AdminsCredentials", b =>
                {
                    b.Navigation("Admin");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.AdminsPermission", b =>
                {
                    b.Navigation("Admins");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.City", b =>
                {
                    b.Navigation("Stores");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Customer", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Order", b =>
                {
                    b.Navigation("OrderItems");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.OrderStatus", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Product", b =>
                {
                    b.Navigation("DataStoreProducts");

                    b.Navigation("OrderItems");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductBrand", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductType", b =>
                {
                    b.Navigation("ProductUnderTypes");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.ProductUnderType", b =>
                {
                    b.Navigation("DataStoreProducts");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.Store", b =>
                {
                    b.Navigation("Customers");

                    b.Navigation("DataStoreProducts");

                    b.Navigation("Orders");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("OMarket.Domain.Entities.StoreAddress", b =>
                {
                    b.Navigation("Store");
                });
#pragma warning restore 612, 618
        }
    }
}
