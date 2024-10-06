using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OMarket.Domain.Entities;
using OMarket.Domain.Settings;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Application.Services.Migration
{
    public class MigrationService : IHostedService
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<MigrationService> _logger;

        private readonly IOptions<DatabaseInitialDataSettings> _initialDataOptions;

        public MigrationService(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<MigrationService> logger,
                IOptions<DatabaseInitialDataSettings> initialDataOptions
            )
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _initialDataOptions = initialDataOptions ?? throw new ArgumentNullException(nameof(initialDataOptions));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration...");

            try
            {
                bool isInit = await MigrateAsync(cancellationToken);
                if (isInit)
                {
                    await InitializeDataAsync(cancellationToken);
                }
                _logger.LogInformation("Database migration and initialization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred during database migration: {Message}", ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<bool> MigrateAsync(CancellationToken cancellationToken)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.Database.MigrateAsync(cancellationToken);
            bool isInit = await context.AdminsPermissions.AnyAsync(cancellationToken);
            return !isInit;
        }

        private async Task InitializeDataAsync(CancellationToken cancellationToken)
        {
            var initialData = _initialDataOptions.Value;

            var adminsPermissions = new HashSet<AdminsPermission>(
                initialData.AdminsPermissions.Select(permission => new AdminsPermission { Permission = permission }));

            var orderStatuses = new HashSet<OrderStatus>(
                initialData.OrderStatuses.Select(status => new OrderStatus { Status = status }));

            var productTypes = InitializeProductTypesAsync(initialData, cancellationToken);

            using var context = _contextFactory.CreateDbContext();

            await context.AdminsPermissions.AddRangeAsync(adminsPermissions, cancellationToken);
            await context.OrderStatuses.AddRangeAsync(orderStatuses, cancellationToken);
            await context.ProductTypes.AddRangeAsync(productTypes, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            if (initialData.Admins.Count != 1)
            {
                throw new ArgumentException(nameof(initialData.Admins));
            }

            string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(initialData.Admins[0].Hash, 12, BCrypt.Net.HashType.SHA256);

            await SaveNewSuperAdminAsync(context, initialData.Admins[0].Login, hash, cancellationToken);
        }

        private List<ProductType> InitializeProductTypesAsync(DatabaseInitialDataSettings initialData, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var productTypes = new List<ProductType>();

            foreach (var type in initialData.TypesProducts)
            {
                var productType = new ProductType { TypeName = type };

                List<ProductUnderType> productUnderTypes = new();

                if (!initialData.ProductUnderTypes.TryGetValue(type, out var value))
                {
                    throw new ArgumentException(nameof(type));
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

                productTypes.Add(productType);
            }

            token.ThrowIfCancellationRequested();

            return productTypes;
        }

        private async Task SaveNewSuperAdminAsync(AppDBContext context, string login, string hash, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(hash))
            {
                throw new ArgumentException("login or hash");
            }

            var existingAdmin = await context.Admins
                .AnyAsync(a => a.AdminsCredentials.Login == login || a.AdminsPermission.Permission == "SuperAdmin", cancellationToken);

            if (existingAdmin)
            {
                throw new ArgumentException("existingAdmin");
            }

            var superAdminPermission = await context.AdminsPermissions
                .SingleOrDefaultAsync(p => p.Permission == "SuperAdmin", cancellationToken)
                ?? throw new ArgumentException("superAdminPermission");

            Domain.Entities.Admin superAdmin = new()
            {
                AdminsPermission = superAdminPermission,
                AdminsCredentials = new AdminsCredentials { Login = login, Hash = hash }
            };

            await context.Admins.AddAsync(superAdmin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
