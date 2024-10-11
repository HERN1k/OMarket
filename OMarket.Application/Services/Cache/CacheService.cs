using System.Text.Json;

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

        private readonly IDatabase _database;

        private readonly IMemoryCache _memoryCache;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly ILogger<CacheService> _logger;

        private readonly List<string> _keys = new()
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

        private readonly TimeSpan _expiry = TimeSpan.FromMinutes(30.0D);

        public CacheService(
                IConnectionMultiplexer redis,
                IMemoryCache memoryCache,
                IStaticCollectionsService staticCollections,
                ILogger<CacheService> logger
            )
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = _redis.GetDatabase();
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _staticCollections = staticCollections ?? throw new ArgumentNullException(nameof(staticCollections));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SetCacheAsync<T>(string key, T input) where T : class
        {
            if (string.IsNullOrEmpty(key) || input is null)
            {
                return false;
            }

            RedisValue value = JsonSerializer.Serialize<T>(input);

            if (value.IsNullOrEmpty)
            {
                return false;
            }

            return await _database.StringSetAsync(key, value, _expiry);
        }

        public async Task<bool> SetStringCacheAsync(string key, string input)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(input))
            {
                return false;
            }

            return await _database.StringSetAsync(key, input, _expiry);
        }

        public async Task<bool> RemoveCacheAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return await _database.KeyDeleteAsync(key);
        }

        public async Task<T?> GetCacheAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var stringValue = await _database.StringGetAsync(key);

            if (stringValue.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(stringValue!);
        }

        public async Task<string> GetStringCacheAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var stringValue = await _database.StringGetAsync(key);

            if (stringValue.IsNullOrEmpty)
            {
                return string.Empty;
            }

            return stringValue.ToString();
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
            foreach (var key in _keys)
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