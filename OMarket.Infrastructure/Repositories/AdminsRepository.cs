using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class AdminsRepository : IAdminsRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<AdminsRepository> _logger;

        public AdminsRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<AdminsRepository> logger
            )
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<Guid?> GetAdmin(long id, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                return await context.Admins
                    .AsNoTracking()
                    .Where(admin => admin.TgAccountId == id)
                    .Select(admin => admin.Id)
                    .SingleOrDefaultAsync(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }
    }
}