using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

using StackExchange.Redis;

namespace OMarket.Application.Services.HealthCheck
{
    public class HealthCheckService : IHostedService
    {
        private readonly IDbContextFactory<AppDBContext> _dbContextFactory;

        private readonly IConnectionMultiplexer _redis;

        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(
                IDbContextFactory<AppDBContext> dbContextFactory,
                IConnectionMultiplexer redis,
                ILogger<HealthCheckService> logger
            )
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                await context.Database.OpenConnectionAsync(cancellationToken);
                await context.Database.CloseConnectionAsync();
                _logger.LogInformation("Database connection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Database connection failed: {Message}", ex.Message);
                throw;
            }

            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                if (!server.IsConnected)
                {
                    throw new InvalidOperationException("Unable to connect to Redis.");
                }
                await _redis.GetDatabase().PingAsync();
                _logger.LogInformation("Redis connection successful.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Redis connection failed: {Message}", ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
