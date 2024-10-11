namespace OMarket.Domain.Interfaces.Application.Services.Cache
{
    public interface ICacheService
    {
        Task<bool> SetCacheAsync<T>(string key, T input) where T : class;

        Task<bool> SetStringCacheAsync(string key, string input);

        Task<bool> RemoveCacheAsync(string key);

        Task<T?> GetCacheAsync<T>(string key) where T : class;

        Task<string> GetStringCacheAsync(string key);

        Task ClearAndUpdateCacheAsync();

        Task UpdateStaticCollectionsAsync();

        void ClearMemoryCache();

        Task ClearRedisCacheAsync();
    }
}