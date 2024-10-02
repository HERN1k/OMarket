using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Helpers.Utilities;

using StackExchange.Redis;

namespace OMarket.Application.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;

        private readonly IMemoryCache _memoryCache;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly ILogger<CacheService> _logger;

        private readonly List<string> keys = new()
        {
            $"{CacheKeys.KeyboardMarkupSelectStoreAddress}updatestoreaddress",
            $"{CacheKeys.KeyboardMarkupSelectStoreAddress}savestoreaddress",
            CacheKeys.KeyboardMarkupMenuProductTypes,
            CacheKeys.KeyboardMarkupSelectProductTypeForCustomerSearchChoice,
            CacheKeys.SelectStoreAddressWithLocationId,
            CacheKeys.SelectStoreAddressWithContactsId,
            CacheKeys.KeyboardMarkupProfileId,
            CacheKeys.SelectStoreAddressUpdateId,
            CacheKeys.SelectStoreAddressForAddReviewId,
            CacheKeys.SelectStoreAddressForViewReviewId,
            CacheKeys.NoReviewsHaveBeenViewId,
            CacheKeys.SelectStoreAddressForConsultationId,
            CacheKeys.RemoveThisMessageId,
            CacheKeys.CustomerOrdersId
        };

        public CacheService(
                IConnectionMultiplexer redis,
                IMemoryCache memoryCache,
                IStaticCollectionsService staticCollections,
                ILogger<CacheService> logger
            )
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _staticCollections = staticCollections ?? throw new ArgumentNullException(nameof(staticCollections));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ClearAndUpdateCacheAsync()
        {
            ClearMemoryCache();

            await ClearRedisCacheAsync();

            await UpdateStaticCollectionsAsync();
        }

        public async Task UpdateStaticCollectionsAsync()
        {
            await _staticCollections.UpdateStaticCollectionsAsync();
        }

        public void ClearMemoryCache()
        {
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }
        }

        public async Task ClearRedisCacheAsync()
        {
            try
            {
                IServer server = GetRedisServer();

                await server.FlushDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Redis exception: {Message}", ex.Message);
                throw new ApplicationException();
            }
        }

        private IServer GetRedisServer()
        {
            var endpoint = _redis.GetEndPoints()[0];
            return _redis.GetServer(endpoint);
        }
    }
}