namespace IPSB.Cache
{
    public class CacheKey<T>
    {
        private readonly int _cacheId;
        private readonly string _cacheObjectType;

        public CacheKey(int cacheId)
        {
            _cacheId = cacheId;
            _cacheObjectType = typeof(T).Name;
        }

        public string CacheId => $"{_cacheObjectType}_{_cacheId}";
        public string CacheAll => $"{_cacheObjectType}_All";
        public string CacheIDTime => $"{_cacheObjectType}_{_cacheId}_Time";
        public string CacheAllTime => $"{_cacheObjectType}_All_Time";
    }
}
