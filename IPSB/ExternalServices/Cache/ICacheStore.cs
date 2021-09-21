using System;
using System.Linq;
using System.Threading.Tasks;
using IPSB.Utils;

namespace IPSB.Cache
{
    public interface ICacheStore
    {
        Task<TItem> GetOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<TItem>> func, string ifModifiedSince);
        Task<Paged<TItem>> GetAllOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<Paged<TItem>>> func, string ifModifiedSince);
        Task Remove<TItem>(CacheKey<TItem> key);
    }
}
