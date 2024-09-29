using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.Entities;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<StoreRepository> _logger;

        public StoreRepository(
            IDbContextFactory<AppDBContext> contextFactory,
            ILogger<StoreRepository> logger
          )
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<string> SetStoreChatIdAsync(Guid adminId, long chatId, CancellationToken token)
        {
            if (adminId == Guid.Empty)
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                Store store = await context.Stores
                    .Where(store => store.AdminId == adminId)
                    .SingleOrDefaultAsync(token)
                        ?? throw new TelegramException();

                store.TgChatId = chatId;

                string result = store.Id.ToString();

                await context.SaveChangesAsync(token);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TelegramException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task<long?> GetStoreChatIdAsync(Guid storeId, CancellationToken token)
        {
            if (storeId == Guid.Empty)
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                return await context.Stores
                    .AsNoTracking()
                    .Where(store => store.Id == storeId)
                    .Select(store => store.TgChatId)
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