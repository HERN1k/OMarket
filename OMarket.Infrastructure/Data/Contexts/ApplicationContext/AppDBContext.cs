using Microsoft.EntityFrameworkCore;

using OMarket.Domain.Entities;
using OMarket.Infrastructure.Data.BuildEntities;

namespace OMarket.Infrastructure.Data.Contexts.ApplicationContext
{
    public class AppDBContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<StoreAddress> StoreAddresses { get; set; }
        public DbSet<AdminsPermission> AdminsPermissions { get; set; }
        public DbSet<AdminsCredentials> AdminsCredentials { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<StoreTelegramChat> StoreTelegramChats { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductUnderType> ProductUnderTypes { get; set; }
        public DbSet<DataStoreProduct> DataStoreProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.BuildCustomerEntity();
            modelBuilder.BuildCityEntity();
            modelBuilder.BuildStoreAddressEntity();
            modelBuilder.BuildAdminsPermissionEntity();
            modelBuilder.BuildAdminsCredentialsEntity();
            modelBuilder.BuildOrderStatusEntity();
            modelBuilder.BuildStoreEntity();
            modelBuilder.BuildAdminEntity();
            modelBuilder.BuildStoreTelegramChatEntity();
            modelBuilder.BuildOrderEntity();
            modelBuilder.BuildProductTypeEntity();
            modelBuilder.BuildProductBrandEntity();
            modelBuilder.BuildProductEntity();
            modelBuilder.BuildOrderItemEntity();
            modelBuilder.BuildProductUnderTypeEntity();
            modelBuilder.BuildDataStoreProductEntity();

            base.OnModelCreating(modelBuilder);
        }

        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        { }
    }
}