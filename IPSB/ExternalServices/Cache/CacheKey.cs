namespace IPSB.Cache
{
    public class CacheKey<T>
    {
        private readonly int _cacheId;
        private readonly string _cacheObjectType;
        private readonly string _cacheIdString;

        public CacheKey(int cacheId)
        {
            _cacheId = cacheId;
            _cacheObjectType = typeof(T).Name;
        }

        public CacheKey(string cacheId)
        {
            _cacheIdString = cacheId;
            _cacheObjectType = typeof(T).Name;
        }
        public string CacheId => $"{_cacheObjectType}_{_cacheId}";
        public string CacheIDTime => $"{_cacheObjectType}_{_cacheId}_Time";
        public string CacheAll => $"{_cacheObjectType}_All";
        public string CacheAllTime => $"{_cacheObjectType}_All_Time";
        public string CacheIdString => $"{_cacheObjectType}_{_cacheIdString}";
        public string CacheIdStingTime => $"{_cacheObjectType}_{_cacheIdString}_Time";
    }
}
