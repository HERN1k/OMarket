namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IStoreRepository
    {
        Task<string> SetStoreChatIdAsync(Guid adminId, long chatId, CancellationToken token);

        Task<long?> GetStoreChatIdAsync(Guid storeId, CancellationToken token);
    }
}