using Application.Interfaces.External;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out T? value) ? value : default);
        }

        public async Task RemoveAsync(string key)
        {
            await Task.Run(() => _memoryCache.Remove(key));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            _memoryCache.Set(key, value, expiration ?? TimeSpan.FromHours(1));
            await Task.CompletedTask;
        }
    }
}
