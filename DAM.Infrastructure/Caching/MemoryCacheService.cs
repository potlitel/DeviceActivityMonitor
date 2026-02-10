using Microsoft.Extensions.Caching.Memory;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key) => await Task.FromResult(cache.Get<T>(key));

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        cache.Set(key, value, expiration ?? TimeSpan.FromMinutes(10));
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key) { cache.Remove(key); await Task.CompletedTask; }
}