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
        public static void ApplyMigrations(IApplicationBuilder app)
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
            //HashSet<City> citySet = new();

            //HashSet<StoreAddress> storeAddressesSet = new();

            HashSet<AdminsPermission> adminsPermissionsSet = new();

            (string Login, string Hash) admin = new();

            HashSet<OrderStatus> orderStatusesSet = new();

            //HashSet<Store> storesSet = new();

            HashSet<ProductType> productTypesSet = new();
            #endregion

            #region Mapping entities collections
            if (initialData.Initialize)
            {
                #region Mapping cities entities
                //foreach (string item in initialData.Cities)
                //{
                //    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                //    citySet.Add(new City() { CityName = item });
                //}
                #endregion

                #region Mapping store addresses entities
                //foreach (StoreAddresses item in initialData.StoreAddresses)
                //{
                //    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));

                //    storeAddressesSet.Add(new StoreAddress()
                //    {
                //        Address = item.Address,
                //        Latitude = item.Latitude,
                //        Longitude = item.Longitude
                //    });
                //}
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
                    ArgumentException.ThrowIfNullOrEmpty(item.Login, nameof(item.Login));
                    ArgumentException.ThrowIfNullOrEmpty(item.Hash, nameof(item.Hash));

                    string hash = BCrypt.Net.BCrypt
                        .EnhancedHashPassword(item.Hash, 12, BCrypt.Net.HashType.SHA256);

                    admin.Login = item.Login;
                    admin.Hash = hash;
                }
                #endregion

                #region Mapping order statuses entities
                foreach (string item in initialData.OrderStatuses)
                {
                    ArgumentException.ThrowIfNullOrEmpty(item, nameof(item));

                    orderStatusesSet.Add(new OrderStatus() { Status = item });
                }
                #endregion

                #region Mapping stores entities
                //foreach (Stores item in initialData.Stores)
                //{
                //    ArgumentException.ThrowIfNullOrEmpty(item.PhoneNumber, nameof(item.PhoneNumber));
                //    ArgumentException.ThrowIfNullOrEmpty(item.Address, nameof(item.Address));
                //    ArgumentException.ThrowIfNullOrEmpty(item.City, nameof(item.City));

                //    storesSet.Add(new Store()
                //    {
                //        Address = storeAddressesSet.SingleOrDefault(e => e.Address == item.Address)
                //            ?? throw new ArgumentException("Incorrect address name.", nameof(item.Address)),

                //        City = citySet.SingleOrDefault(e => e.CityName == item.City)
                //            ?? throw new ArgumentException("Incorrect city name.", nameof(item.City)),

                //        PhoneNumber = item.PhoneNumber
                //    });
                //}
                #endregion

                #region Mapping product types entities
                foreach (string type in initialData.TypesProducts)
                {
                    ArgumentException.ThrowIfNullOrEmpty(type, nameof(type));

                    ProductType productType = new()
                    {
                        TypeName = type
                    };

                    List<ProductUnderType> productUnderTypes = new();

                    if (!initialData.ProductUnderTypes.TryGetValue(type, out var value))
                    {
                        throw new ArgumentNullException(nameof(type));
                    }

                    foreach (string item in value)
                    {
                        productUnderTypes.Add(new ProductUnderType()
                        {
                            UnderTypeName = item,
                            ProductType = productType
                        });
                    }

                    productType.ProductUnderTypes = productUnderTypes;

                    productTypesSet.Add(productType);
                }
                #endregion
            }
            #endregion

            try
            {
                Migrate(contextFactory);

                #region Save entities collection
                if (initialData.Initialize)
                {
                    CreateNewEntities(
                        contextFactory,
                        //citySet,
                        //storeAddressesSet,
                        adminsPermissionsSet,
                        orderStatusesSet,
                        //storesSet,
                        productTypesSet);

                    ArgumentException.ThrowIfNullOrEmpty(admin.Login, nameof(admin.Login));
                    ArgumentException.ThrowIfNullOrEmpty(admin.Hash, nameof(admin.Hash));

                    SaveNewSuperAdmin(contextFactory, admin.Login, admin.Hash);
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

        public static void Migrate(IDbContextFactory<AppDBContext> contextFactory)
        {
            using AppDBContext context = contextFactory.CreateDbContext();

            context.Database.Migrate();

            context.SaveChanges();
        }

        public static void CreateNewEntities(
            IDbContextFactory<AppDBContext> contextFactory,
            //HashSet<City> citySet,
            //HashSet<StoreAddress> storeAddressesSet,
            HashSet<AdminsPermission> adminsPermissionsSet,
            HashSet<OrderStatus> orderStatusesSet,
            //HashSet<Store> storesSet,
            HashSet<ProductType> productTypesSet)
        {
            using AppDBContext context = contextFactory.CreateDbContext();

            //context.Cities.AddRange(citySet);
            //context.StoreAddresses.AddRange(storeAddressesSet);
            context.AdminsPermissions.AddRange(adminsPermissionsSet);
            context.OrderStatuses.AddRange(orderStatusesSet);
            //context.Stores.AddRange(storesSet);
            context.ProductTypes.AddRange(productTypesSet);

            context.SaveChanges();
        }

        public static void SaveNewSuperAdmin(IDbContextFactory<AppDBContext> contextFactory, string login, string hash)
        {
            ArgumentException.ThrowIfNullOrEmpty(login, nameof(login));
            ArgumentException.ThrowIfNullOrEmpty(hash, nameof(hash));

            using AppDBContext context = contextFactory.CreateDbContext();

            bool isNewSuperAdmin = context.Admins
                .Where(admin => admin.AdminsPermission.Permission == "SuperAdmin")
                .Any();

            bool isNewAdminLogin = context.Admins
                .Where(admin => admin.AdminsCredentials.Login == login)
                .Any();

            if (isNewAdminLogin || isNewSuperAdmin)
            {
                return;
            }

            AdminsPermission? adminPermission = context.AdminsPermissions
                .Where(permission => permission.Permission == "SuperAdmin")
                .SingleOrDefault();

            if (adminPermission is null)
            {
                return;
            }

            AdminsCredentials adminCredentials = new() { Login = login, Hash = hash };

            Admin admin = new()
            {
                AdminsPermission = adminPermission,
                AdminsCredentials = adminCredentials
            };

            context.Add(admin);

            context.SaveChanges();
        }
    }
}