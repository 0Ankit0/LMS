using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace LMS.Web.Services
{
    public class CachingService
    {
        private readonly IMemoryCache _cache;

        public CachingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null)
        {
            if (!_cache.TryGetValue(key, out T result))
            {
                result = await factory();
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(5)
                };
                _cache.Set(key, result, cacheEntryOptions);
            }
            return result;
        }
    }
}
