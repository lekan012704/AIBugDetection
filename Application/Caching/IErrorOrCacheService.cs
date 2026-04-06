using ErrorOr;

namespace Application.Caching
{
    public interface IErrorOrCacheService
    {
        Task<ErrorOr<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<ErrorOr<T>> SetAsync<T>(string key,T value,TimeSpan? expiration = null,
            CancellationToken cancellationToken = default);
        Task<ErrorOr<Success>> RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
