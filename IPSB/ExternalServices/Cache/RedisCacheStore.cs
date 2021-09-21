using IPSB.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.Cache
{
    public class RedisCacheStore : ICacheStore
    {
        private readonly IDistributedCache _distributedCache;
        private readonly Dictionary<string, TimeSpan> _expirationConfiguration;

        public RedisCacheStore(IDistributedCache distributedCache, Dictionary<string, TimeSpan> expirationConfiguration)
        {
            _distributedCache = distributedCache;
            _expirationConfiguration = expirationConfiguration;
        }

        public async Task<TItem> GetOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<TItem>> func, string ifModifiedSince)
        {
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

            string updateTime = DateTime.Now.ToString("ddd, dd MMM yyy HH:mm:ss") + " GMT";

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

        public async Task<IQueryable<TItem>> GetAllOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<IQueryable<TItem>>> func, string ifModifiedSince)
        {
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
                        throw new Exception("Not-modified");
                    }
                }

                return JsonConvert.DeserializeObject<IQueryable<TItem>>(cachedItem);

                /* JsonSerializer.Deserialize<TItem>(cachedItem) can not be used here
                 * because it caused "A possible object cycle was detected" exception.*/

            }

            string updateTime = DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’");

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

        public async Task Remove<TItem>(CacheKey<TItem> key)
        {
            var cachedItem = await _distributedCache.GetStringAsync(key.CacheId);
            if (!string.IsNullOrEmpty(cachedItem))
            {
                await _distributedCache.RemoveAsync(key.CacheId);
                await _distributedCache.RemoveAsync(key.CacheIDTime);

                var cachedAllItem = await _distributedCache.GetStringAsync(key.CacheAll);

                if (!string.IsNullOrEmpty(cachedAllItem))
                {
                    await _distributedCache.RemoveAsync(key.CacheAll);
                    await _distributedCache.RemoveAsync(key.CacheAllTime);
                }
            }

        }
    }
}

