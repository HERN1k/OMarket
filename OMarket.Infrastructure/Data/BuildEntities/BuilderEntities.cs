﻿using Microsoft.EntityFrameworkCore;

using OMarket.Domain.Entities;
using OMarket.Helpers.Extensions;

namespace OMarket.Infrastructure.Data.BuildEntities
{
    internal static class BuilderEntities
    {
        public static void BuildCustomerEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasOne(e => e.City)
                    .WithMany(e => e.Customers)
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.StoreAddress)
                    .WithMany(e => e.Customers)
                    .HasForeignKey(e => e.StoreAddressId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .IsRequired();

                entity.Property(e => e.Username)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(5)
                    .HasMaxLength(32);

                entity.Property(e => e.FirstName)
                    .HasColumnType("varchar(64)")
                    .HasMinLength(1)
                    .HasMaxLength(64)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnType("varchar(64)")
                    .HasMaxLength(64);

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("varchar(16)")
                    .HasMinLength(10)
                    .HasMaxLength(16);

                entity.Property(e => e.CityId)
                    .HasColumnType("uuid");

                entity.Property(e => e.IsBot)
                    .HasColumnType("boolean")
                    .IsRequired();

                entity.Property(e => e.StoreAddressId)
                    .HasColumnType("uuid");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();
            });
        }

        public static void BuildCityEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("Cities");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.CityName);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.CityName)
                    .HasColumnType("varchar(64)")
                    .HasMinLength(2)
                    .HasMaxLength(64)
                    .IsRequired();
            });
        }

        public static void BuildStoreAddressEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreAddress>(entity =>
            {
                entity.ToTable("StoreAddresses");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasOne(e => e.Store)
                    .WithOne(s => s.Address)
                    .HasForeignKey<Store>(e => e.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Address)
                    .HasColumnType("varchar(256)")
                    .HasMinLength(3)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(e => e.Latitude)
                    .HasColumnType("decimal(9, 6)")
                    .IsRequired();

                entity.Property(e => e.Longitude)
                    .HasColumnType("decimal(9, 6)")
                    .IsRequired();
            });
        }

        public static void BuildAdminsPermissionEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminsPermission>(entity =>
            {
                entity.ToTable("AdminsPermissions");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Permission)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(3)
                    .HasMaxLength(32)
                    .IsRequired();
            });
        }

        public static void BuildAdminsCredentialsEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminsCredentials>(entity =>
            {
                entity.ToTable("AdminsCredentials");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Login)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(5)
                    .HasMaxLength(32)
                    .IsRequired();

                entity.Property(e => e.Hash)
                    .HasColumnType("char(64)")
                    .IsRequired();
            });
        }

        public static void BuildOrderStatusEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("OrderStatuses");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(3)
                    .HasMaxLength(32)
                    .IsRequired();
            });
        }

        public static void BuildStoreEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Stores");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.CityId);

                entity.HasOne(e => e.City)
                    .WithMany(e => e.Stores)
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Address)
                    .WithOne(e => e.Store)
                    .HasForeignKey<Store>(e => e.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                    .WithOne(e => e.Store)
                    .HasForeignKey<Store>(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.StoreTelegramChat)
                    .WithOne(e => e.Store)
                    .HasForeignKey<Store>(e => e.StoreTelegramChatId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.AddressId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.CityId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.AdminId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.StoreTelegramChatId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("varchar(16)")
                    .HasMinLength(10)
                    .HasMaxLength(16)
                    .IsRequired();
            });
        }

        public static void BuildAdminEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admins");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasOne(e => e.AdminsPermission)
                    .WithMany(c => c.Admins)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AdminsCredentials)
                    .WithOne(a => a.Admin)
                    .HasForeignKey<Admin>(e => e.CredentialsId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.PermissionId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.CredentialsId)
                    .HasColumnType("uuid")
                    .IsRequired();
            });
        }

        public static void BuildStoreTelegramChatEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreTelegramChat>(entity =>
            {
                entity.ToTable("StoreTelegramChats");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.ChatId);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.ChatId)
                    .HasColumnType("bigint")
                    .IsRequired();
            });
        }

        public static void BuildOrderEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(e => e.Customer)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Store)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrderStatus)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.CustomerId)
                    .HasColumnType("bigint")
                    .IsRequired();

                entity.Property(e => e.StoreId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(10, 2)")
                    .IsRequired();

                entity.Property(e => e.StatusId)
                    .HasColumnType("uuid")
                    .IsRequired();
            });
        }

        public static void BuildProductTypeEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.ToTable("ProductTypes");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.TypeName);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.TypeName)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(1)
                    .HasMaxLength(32)
                    .IsRequired();
            });
        }

        public static void BuildProductBrandEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductBrand>(entity =>
            {
                entity.ToTable("ProductBrands");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.BrandName);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.BrandName)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(1)
                    .HasMaxLength(32)
                    .IsRequired();
            });
        }

        public static void BuildProductUnderTypeEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductUnderType>(entity =>
            {
                entity.ToTable("ProductUnderTypes");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.UnderTypeName);

                entity.HasIndex(e => e.ProductTypeId);

                entity.HasOne(e => e.ProductType)
                    .WithMany(e => e.ProductUnderTypes)
                    .HasForeignKey(e => e.ProductTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.ProductTypeId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.UnderTypeName)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(1)
                    .HasMaxLength(32)
                    .IsRequired();
            });
        }

        public static void BuildProductEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.Name);

                entity.HasIndex(e => e.TypeId);

                entity.HasIndex(e => e.UnderTypeId);

                entity.HasIndex(e => e.BrandId);

                entity.HasOne(e => e.ProductType)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProductUnderType)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.UnderTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProductBrand)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(32)")
                    .HasMinLength(1)
                    .HasMaxLength(32)
                    .IsRequired();

                entity.Property(e => e.PhotoUri)
                    .HasColumnType("varchar(128)")
                    .HasMinLength(1)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.Property(e => e.TypeId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.UnderTypeId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.BrandId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .IsRequired();

                entity.Property(e => e.Dimensions)
                    .HasColumnType("varchar(32)")
                    .HasMaxLength(32);

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(64)")
                    .HasMaxLength(64);
            });
        }

        public static void BuildOrderItemEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.OrderId);

                entity.HasOne(e => e.Order)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.OrderId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.ProductId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Quantity)
                    .HasColumnType("integer")
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(10, 2)")
                    .IsRequired();

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(10, 2)")
                    .IsRequired();
            });
        }

        public static void BuildDataStoreProductEntity(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataStoreProduct>(entity =>
            {
                entity.ToTable("DataStoreProducts");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id)
                    .IsUnique();

                entity.HasIndex(e => e.ProductId);

                entity.HasIndex(e => e.StoreId);

                entity.HasIndex(e => e.Status);

                entity.HasIndex(e => e.ProductUnderTypeId);

                entity.HasOne(e => e.Product)
                    .WithMany(e => e.DataStoreProducts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Store)
                    .WithMany(e => e.DataStoreProducts)
                    .HasForeignKey(e => e.StoreId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductUnderType)
                    .WithMany(e => e.DataStoreProducts)
                    .HasForeignKey(e => e.ProductUnderTypeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Setting property 

                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.ProductId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.StoreId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.ProductUnderTypeId)
                    .HasColumnType("uuid")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnType("boolean")
                    .IsRequired();
            });
        }
    }
}