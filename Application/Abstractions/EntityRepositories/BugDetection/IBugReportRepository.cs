using Application.Abstractions.GenericRepository;
using Domain.Application.Entities.BugDetection;
using ErrorOr;

namespace Application.Abstractions.EntityRepositories.BugDetection;

public interface IBugReportRepository
    : IRepositoryAsync<BugReport, Guid>
{
    Task<ErrorOr<BugReport>> GetByIdWithItemsAsync(
        Guid reportId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<List<BugReport>>> GetByUserIdAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<int>> GetTotalCountByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);
}