
using IPSB.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IPSB.Cache
{
    public class RedisCacheStore : ICacheStore
    {
        private readonly IDistributedCache _distributedCache;
        private readonly Dictionary<string, TimeSpan> _expirationConfiguration;
        private readonly IConfiguration _config;

        public RedisCacheStore(IDistributedCache distributedCache, Dictionary<string, TimeSpan> expirationConfiguration, IConfiguration config)
        {
            _distributedCache = distributedCache;
            _expirationConfiguration = expirationConfiguration;
            _config = config;
        }

        public async Task<TItem> GetOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<TItem>> func, string ifModifiedSince)
        {
            if (_config[Constants.CacheConfig.CACHE_STATUS].Equals("off"))
            {
                return await func("");
            }
            var cachedObjectName = item.GetType().Name;
            var cachedItem = await _distributedCache.GetStringAsync(key.CacheId);
            var cachedItemTime = await _distributedCache.GetStringAsync(key.CacheIDTime);
            var timespan = _expirationConfiguration[cachedObjectName];


            if (!string.IsNullOrEmpty(cachedItem))
            {
                if (!string.IsNullOrEmpty(cachedItemTime))
                {
                    if (cachedItemTime.Equals(ifModifiedSince))
                    {
                        throw new Exception(Constants.ExceptionMessage.NOT_MODIFIED);
                    }
                }

                return JsonConvert.DeserializeObject<TItem>(cachedItem);

                /* JsonSerializer.Deserialize<TItem>(cachedItem) can not be used here
                 * because it caused "A possible object cycle was detected" exception.*/

            }

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            string updateTime = localTime.DateTime.ToString("ddd, dd MMM yyy HH:mm:ss") + " GMT";

            var newItem = await func(updateTime);

            if (newItem != null)
            {

                var cacheEntryOptions = new DistributedCacheEntryOptions()
                                        .SetSlidingExpiration(timespan);

                string serializedItem =
                    JsonConvert.SerializeObject(newItem, Formatting.Indented, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                /* JsonSerializer.Serialize(newItem) can not be used here
                * because it caused "A possible object cycle was detected" exception.*/

                await _distributedCache.SetStringAsync(key.CacheId, serializedItem, cacheEntryOptions);
                await _distributedCache.SetStringAsync(key.CacheIDTime, updateTime.ToString(), cacheEntryOptions);
            }

            return newItem;
        }

        public async Task<CacheResponse<TItem>> GetAllOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<IQueryable<TItem>>> func, Func<string, string> setLastModified, string ifModifiedSince)
        {
            CacheResponse<TItem> response = new CacheResponse<TItem>();
            if (_config[Constants.CacheConfig.CACHE_STATUS].Equals("off"))
            {
                response.Result = await func("");
                return response;
            }
            var cachedObjectName = item.GetType().Name;
            var cachedItem = await _distributedCache.GetStringAsync(key.CacheAll);
            var cachedItemTime = await _distributedCache.GetStringAsync(key.CacheAllTime);
            var timespan = _expirationConfiguration[cachedObjectName];



            if (!string.IsNullOrEmpty(cachedItem))
            {
                if (!string.IsNullOrEmpty(cachedItemTime))
                {
                    if (cachedItemTime.Equals(ifModifiedSince))
                    {
                        response.NotModified = true;
                    }
                }
                setLastModified(cachedItemTime);
                response.Result = JsonConvert.DeserializeObject<List<TItem>>(cachedItem).AsQueryable();
                return response;
                /* JsonSerializer.Deserialize<TItem>(cachedItem) can not be used here
                 * because it caused "A possible object cycle was detected" exception.*/

            }

            var info = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            string updateTime = localTime.DateTime.ToString("ddd, dd MMM yyy HH:mm:ss") + " GMT";

            var newItem = await func(updateTime);
            response.Result = newItem;
            if (newItem != null)
            {

                var cacheEntryOptions = new DistributedCacheEntryOptions()
                                        .SetSlidingExpiration(timespan);

                string serializedItem =
                    JsonConvert.SerializeObject(newItem, Formatting.Indented, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                /* JsonSerializer.Serialize(newItem) can not be used here
                * because it caused "A possible object cycle was detected" exception.*/

                await _distributedCache.SetStringAsync(key.CacheAll, serializedItem, cacheEntryOptions);
                await _distributedCache.SetStringAsync(key.CacheAllTime, updateTime.ToString(), cacheEntryOptions);
            }

            return response;
        }

        public async Task Remove<TItem>(CacheKey<TItem> key)
        {
            var cachedItem = await _distributedCache.GetStringAsync(key.CacheId);
            if (!string.IsNullOrEmpty(cachedItem))
            {
                await _distributedCache.RemoveAsync(key.CacheId);
                await _distributedCache.RemoveAsync(key.CacheIDTime);
            }

            var cachedAllItem = await _distributedCache.GetStringAsync(key.CacheAll);
            if (!string.IsNullOrEmpty(cachedAllItem))
            {
                await _distributedCache.RemoveAsync(key.CacheAll);
                await _distributedCache.RemoveAsync(key.CacheAllTime);
            }

        }

        public async Task Remove<TItem>(int id)
        {
            var cacheId = new CacheKey<TItem>(id);
            await Remove(cacheId);
        }

        public async Task<bool> RemoveAll()
        {
            var keys = GetAllRedisKeys();
            try
            {
                await Task.WhenAll(keys.Select(key => _distributedCache.RemoveAsync(key.ToString())));
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public IEnumerable<string> GetAllKeys()
        {
            var keys = GetAllRedisKeys();
            return keys.Select(_ => _.ToString());
        }

        private RedisKey[] GetAllRedisKeys()
        {
            string connectionString = _config.GetConnectionString("RedisConnectionString");
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(options);
            EndPoint endPoint = connection.GetEndPoints().First();
            RedisKey[] keys = connection.GetServer(endPoint).Keys(pattern: "*").ToArray();
            return keys;
        }

        public async Task<string> GetByKey(string key)
        {
            return await _distributedCache.GetStringAsync(key);
        }
    }
}

