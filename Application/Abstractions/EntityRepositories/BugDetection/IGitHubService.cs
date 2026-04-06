using ErrorOr;

namespace Application.Abstractions.AI;

public interface IGitHubService
{
    Task<ErrorOr<string>> FetchCodeAsync(
        string url,
        CancellationToken cancellationToken);
}