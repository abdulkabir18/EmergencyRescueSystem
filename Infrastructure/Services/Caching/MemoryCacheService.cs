using Application.Interfaces.External;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Infrastructure.Services.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

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
            //await Task.Run(() => _memoryCache.Remove(key));
            await Task.Run(() =>
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            });
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            await Task.Run(() =>
            {
                var keysToRemove = _cacheKeys.Keys
                    .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }
            });
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            //_memoryCache.Set(key, value, expiration ?? TimeSpan.FromHours(1));
            //await Task.CompletedTask;

            _memoryCache.Set(key, value, expiration ?? TimeSpan.FromHours(1));
            _cacheKeys.TryAdd(key, true);
            await Task.CompletedTask;
        }
    }
}
