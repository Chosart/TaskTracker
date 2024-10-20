namespace TaskTracker.Services
{
    public interface IRedisCacheService
    {
        Task SetCacheValueAsync(string key, string value, TimeSpan? expiry = null);
        Task<string> GetCacheValueAsync(string key);
        Task RemoveCacheValueAsync(string key);
        Task<bool> KeyExistsAsync(string key);
    }
}
