using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OMarket.Domain.Entities;
using OMarket.Domain.Settings;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Extensions
{
    public class MigrationExtensions
    {
        public static async void ApplyMigrations(IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            IDbContextFactory<AppDBContext> contextFactory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<AppDBContext>>();

            ILogger<MigrationExtensions> logger = scope.ServiceProvider
                .GetRequiredService<ILogger<MigrationExtensions>>();

            logger.LogInformation("Database migration has started...");

            DatabaseInitialDataSettings initialData = scope.ServiceProvider
                .GetRequiredService<IOptions<DatabaseInitialDataSettings>>().Value;

            #region Entities collections
            HashSet<City> citySet = new();

            HashSet<StoreAddress> storeAddressesSet = new();

            HashSet<AdminsPermission> adminsPermissionsSet = new();

            HashSet<AdminsCredentials> adminsCredentialsSet = new();

            Dictionary<string, Admin> adminsDictionary = new();

            HashSet<OrderStatus> orderStatusesSet = new();

            Dictionary<string, StoreTelegramChat> storeTelegramChatsDictionary = new();

            HashSet<Store> storesSet = new();

            HashSet<ProductType> productTypesSet = new();

            HashSet<ProductUnderType> productUnderTypesSet = new();

            HashSet<ProductBrand> productBrandsSet = new();
            #endregion

            #region Mapping entities collections
            if (initialData.Initialize)
            {
                #region Mapping cities entities
                foreach (string item in initialData.Cities)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    citySet.Add(new City() { CityName = item });
                }
                #endregion

                #region Mapping store addresses entities
                foreach (StoreAddresses item in initialData.StoreAddresses)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));

                    storeAddressesSet.Add(new StoreAddress()
                    {
                        Address = item.Address,
                        Latitude = item.Latitude,
                        Longitude = item.Longitude
                    });
                }
                #endregion

                #region Mapping admins permissions entities
                foreach (string item in initialData.AdminsPermissions)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    adminsPermissionsSet.Add(new AdminsPermission() { Permission = item });
                }
                #endregion

                #region Mapping admins credentials and admins entities
                foreach (Admins item in initialData.Admins)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));
                    ArgumentException.ThrowIfNullOrEmpty(item.Permission, nameof(item.Permission));
                    ArgumentException.ThrowIfNullOrEmpty(item.Login, nameof(item.Login));
                    ArgumentException.ThrowIfNullOrEmpty(item.Hash, nameof(item.Hash));

                    AdminsCredentials credentials = new()
                    {
                        Login = item.Login,
                        Hash = item.Hash
                    };

                    adminsCredentialsSet.Add(credentials);

                    adminsDictionary.Add(item.Address, new Admin()
                    {
                        AdminsPermission = adminsPermissionsSet
                            .SingleOrDefault(e => e.Permission == item.Permission)
                                ?? throw new ArgumentException("Incorrect permission name.", nameof(item.Permission)),

                        AdminsCredentials = credentials
                    });
                }
                #endregion

                #region Mapping order statuses entities
                foreach (string item in initialData.OrderStatuses)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    orderStatusesSet.Add(new OrderStatus() { Status = item });
                }
                #endregion

                #region Mapping stores telegram chats entities
                foreach (StoreTelegramChats item in initialData.StoreTelegramChats)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));

                    storeTelegramChatsDictionary.Add(item.Address, new StoreTelegramChat() { ChatId = item.ChatId });
                }
                #endregion

                #region Mapping stores entities
                foreach (Stores item in initialData.Stores)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item.PhoneNumber, nameof(item.PhoneNumber));
                    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));
                    ArgumentException.ThrowIfNullOrEmpty(item.City, nameof(item.City));

                    storesSet.Add(new Store()
                    {
                        Address = storeAddressesSet.SingleOrDefault(e => e.Address == item.Address)
                            ?? throw new ArgumentException("Incorrect address name.", nameof(item.Address)),

                        City = citySet.SingleOrDefault(e => e.CityName == item.City)
                            ?? throw new ArgumentException("Incorrect city name.", nameof(item.City)),

                        Admin = adminsDictionary.SingleOrDefault(e => e.Key == item.Address).Value
                            ?? throw new ArgumentException("Incorrect address name.", nameof(item.Address)),

                        StoreTelegramChat = storeTelegramChatsDictionary
                            .SingleOrDefault(e => e.Key == item.Address).Value
                                ?? throw new ArgumentException("Incorrect address name.", nameof(item.Address)),

                        PhoneNumber = item.PhoneNumber
                    });
                }
                #endregion

                #region Mapping product types entities
                foreach (string item in initialData.TypesProducts)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    productTypesSet.Add(new ProductType() { TypeName = item });
                }
                #endregion

                #region Mapping products under types entities
                foreach (string item in initialData.ProductUnderTypes)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    productUnderTypesSet.Add(new ProductUnderType() { UnderTypeName = item });
                }
                #endregion

                #region Mapping products brands entities
                foreach (string item in initialData.BrandsProducts)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    productBrandsSet.Add(new ProductBrand() { BrandName = item });
                }
                #endregion
            }
            #endregion

            try
            {
                using AppDBContext context = contextFactory.CreateDbContext();

                context.Database.Migrate();

                #region Save entities collection
                if (initialData.Initialize)
                {
                    await context.Cities.AddRangeAsync(citySet);
                    await context.StoreAddresses.AddRangeAsync(storeAddressesSet);
                    await context.AdminsPermissions.AddRangeAsync(adminsPermissionsSet);
                    await context.AdminsCredentials.AddRangeAsync(adminsCredentialsSet);
                    await context.Admins.AddRangeAsync(adminsDictionary.Values);
                    await context.OrderStatuses.AddRangeAsync(orderStatusesSet);
                    await context.StoreTelegramChats.AddRangeAsync(storeTelegramChatsDictionary.Values);
                    await context.Stores.AddRangeAsync(storesSet);
                    await context.ProductTypes.AddRangeAsync(productTypesSet);
                    await context.ProductUnderTypes.AddRangeAsync(productUnderTypesSet);
                    await context.ProductBrands.AddRangeAsync(productBrandsSet);

                    await context.SaveChangesAsync();
                }
                #endregion
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Database migration exception message: {Message}", ex.Message);
                throw;
            }

            logger.LogInformation("Database migration completed");
        }
    }
}