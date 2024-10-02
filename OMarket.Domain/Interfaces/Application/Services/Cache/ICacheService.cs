namespace OMarket.Domain.Interfaces.Application.Services.Cache
{
    public interface ICacheService
    {
        Task ClearAndUpdateCacheAsync();

        Task UpdateStaticCollectionsAsync();

        void ClearMemoryCache();

        Task ClearRedisCacheAsync();
    }
}