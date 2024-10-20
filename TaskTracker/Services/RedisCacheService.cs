using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;

namespace TaskTracker.Services
{
    public class RedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache) 
        {
            _cache = cache;
        }

        public async Task SetCacheValueAsync(string key, string value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(key, value, options);
        }

        public async Task<string> GetCacheValueAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task RemoveCacheValueAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return value != null;
        }
    }
}
