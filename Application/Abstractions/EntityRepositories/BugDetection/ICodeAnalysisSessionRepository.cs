using Application.Abstractions.GenericRepository;
using Domain.Application.Entities.BugDetection;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;

namespace Application.Abstractions.EntityRepositories.BugDetection;

public interface ICodeAnalysisSessionRepository
    : IRepositoryAsync<CodeAnalysisSession, Guid>
{
    Task<ErrorOr<CodeAnalysisSession>> GetByIdWithDetailsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<List<CodeAnalysisSession>>> GetByUserIdAsync(
        string userId,
        int pageNumber,     
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<int>> GetTotalCountByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<bool>> ExistsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<List<SessionSummaryResponse>>> GetSummariesByUserIdAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}