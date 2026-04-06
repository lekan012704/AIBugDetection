using Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Caching
{
    internal sealed class CacheService(HybridCache hybridCache, ILogger<CacheService> logger) : ICacheService
    {
        private readonly HybridCache _hybridCache = hybridCache;
        private readonly ILogger<CacheService> _logger = logger;
        private const int CacheDefaultExpirationMinutes = 15;
        private const string CacheError = "Cache operation failed";

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
            await _hybridCache.RemoveAsync(key, cancellationToken);

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var options = new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.None,
                Expiration = expiration ?? TimeSpan.FromMinutes(CacheDefaultExpirationMinutes),
                LocalCacheExpiration = expiration ?? TimeSpan.FromMinutes(CacheDefaultExpirationMinutes)
            };

            return await _hybridCache.GetOrCreateAsync(key, async cancel =>
            {
                try
                {
                    // Generate new value if not in cache
                    return await factory();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{CacheError}: Error generating cache value for key {Key}", CacheError, key);
                    throw;
                }
            }, options);

        }

    }
}
