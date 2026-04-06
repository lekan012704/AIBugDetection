using Application.Caching;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;
using System.Buffers;
using System.Text.Json;


namespace Infrastructure.Caching
{
    internal sealed class ErrorOrCacheService : IErrorOrCacheService
    {
        private readonly IDistributedCache _cache;
        private const string CacheError = "Cache operation failed";
        private const int CacheDefaultExpirationMinutes = 15;

        public ErrorOrCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<ErrorOr<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                byte[]? bytes = await _cache.GetAsync(key, cancellationToken);
                if (bytes is null)
                {
                    return Error.NotFound(description: $"Cache key '{key}' not found");
                }

                return Deserialize<T>(bytes);
            }
            catch (Exception ex)
            {
                return Errors.Infrastructure.DatabaseExceptionError(
                    ex,
                    $"{CacheError}: {ex.Message}");
            }
        }

        public async Task<ErrorOr<T>> SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    //AbsoluteExpiration = HybridCacheEntryFlags.None,
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(CacheDefaultExpirationMinutes),
                    SlidingExpiration = expiration ?? TimeSpan.FromMinutes(CacheDefaultExpirationMinutes)
                };

                byte[] bytes = Serialize(value);
                await _cache.SetAsync(key, bytes, options, cancellationToken);
                return value;
            }
            catch (Exception ex)
            {
                return Errors.Infrastructure.DatabaseExceptionError(
                    ex,
                    $"{CacheError}: {ex.Message}");
            }
        }

        public async Task<ErrorOr<Success>> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Errors.Infrastructure.DatabaseExceptionError(
                    ex,
                    $"{CacheError}: {ex.Message}");
            }
        }

        private static ErrorOr<T> Deserialize<T>(byte[] bytes)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(bytes);
                return result!;
            }
            catch (Exception ex)
            {
                return Errors.Infrastructure.DatabaseExceptionError(
                    ex,
                    $"Failed to deserialize cache value: {ex.Message}");
            }
        }

        private static byte[] Serialize<T>(T value)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value);
            return buffer.WrittenSpan.ToArray();
        }
    }
}
