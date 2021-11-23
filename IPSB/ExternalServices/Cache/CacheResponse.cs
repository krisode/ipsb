using System.Linq;

namespace IPSB.Cache
{
    public class CacheResponse<T>
    {
        public IQueryable<T> Result { get; set; }
        public bool NotModified { get; set; }
    }
}
