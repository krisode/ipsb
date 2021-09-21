using System;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.Cache
{
    public interface ICacheStore
    {
        Task<TItem> GetOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<TItem>> func, string ifModifiedSince);
        Task<IQueryable<TItem>> GetAllOrSetAsync<TItem>(TItem item, CacheKey<TItem> key, Func<string, Task<IQueryable<TItem>>> func, Func<string, string> setLastModified, string ifModifiedSince);
        Task Remove<TItem>(CacheKey<TItem> key);
    }
}
