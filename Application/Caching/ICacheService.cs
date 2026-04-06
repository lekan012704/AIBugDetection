namespace Application.Caching
{
    public interface ICacheService
    {
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    }
}
