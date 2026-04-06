using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.BugDetection;
using Domain.Application.Dtos;
using Domain.Application.Dtos.BugDetection;
using Domain.Application.Entities.BugDetection;
using ErrorOr;
using Infrastructure.Database;
using Infrastructure.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.EntityRepositories.BugDetection;

public sealed class CodeAnalysisSessionRepository : RepositoryAsync<CodeAnalysisSession,Guid>, ICodeAnalysisSessionRepository    
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CodeAnalysisSessionRepository> _logger;
    public CodeAnalysisSessionRepository(IUnitOfWork unitOfWork, ILogger<CodeAnalysisSessionRepository> logger) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<CodeAnalysisSession>> GetByIdWithDetailsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {   
        try
        {
            if (sessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            var session = await EntitySet
           .Include(s => s.Issues)
           .Include(s => s.Conversations)
           .AsSplitQuery()
           .FirstOrDefaultAsync(
               s => s.Id == sessionId,
               cancellationToken);

            if (session is null)
                return Errors.Common.NotFound(
                    "Session",
                    sessionId.ToString());

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get session with details {SessionId}",
                sessionId);
            return Errors.Infrastructure.DatabaseError(
                "GetSessionWithDetails", ex.Message);
        }
    }

    public async Task<ErrorOr<List<CodeAnalysisSession>>> GetByUserIdAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Errors.Common.Validation(
                    "UserId",
                    "UserId cannot be empty");

            if (pageNumber < 1)
                return Errors.Common.Validation(
                    "PageNumber",
                    "Page number must be greater than 0");

            if (pageSize < 1)
                return Errors.Common.Validation(
                    "PageSize",
                    "Page size must be greater than 0");

            var sessions = await EntitySet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get sessions for user {UserId}", userId);
            return Errors.Infrastructure.DatabaseError(
                "GetSessionsByUser", ex.Message);
        }
    }

    public async Task<ErrorOr<List<SessionSummaryResponse>>>
        GetSummariesByUserIdAsync(
            string userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Errors.Common.Validation(
                    "UserId",
                    "UserId cannot be empty");

            var summaries = await EntitySet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SessionSummaryResponse
                {
                    SessionId = s.Id,
                    SubmissionType = s.SubmissionType,
                    FileName = s.FileName,
                    GitHubUrl = s.GitHubUrl,
                    Status = s.Status,
                    DetectedLanguage = s.DetectedLanguage,
                    DetectedArchitecture = s.DetectedArchitecture,
                    OverallCodeQuality = s.OverallCodeQuality,
                    TotalIssuesFound = s.TotalIssuesFound,
                    CriticalIssues = s.CriticalIssues,
                    HighIssues = s.HighIssues,
                    MediumIssues = s.MediumIssues,
                    LowIssues = s.LowIssues,
                    AiProvider = s.AiProvider,
                    FallbackUsed = s.FallbackUsed,
                    CreatedAt = s.CreatedAt,
                    FinalAnalyzedAt = s.FinalAnalyzedAt
                })
                .ToListAsync(cancellationToken);

            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get session summaries for user {UserId}",
                userId);
            return Errors.Infrastructure.DatabaseError(
                "GetSummaries", ex.Message);
        }
    }

    public async Task<ErrorOr<int>> GetTotalCountByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Errors.Common.Validation(
                    "UserId",
                    "UserId cannot be empty");

            var count = await EntitySet
                .CountAsync(
                    s => s.UserId == userId,
                    cancellationToken);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get session count for user {UserId}",
                userId);
            return Errors.Infrastructure.DatabaseError(
                "GetSessionCount", ex.Message);
        }
    }

    public async Task<ErrorOr<bool>> ExistsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (sessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            var exists = await EntitySet
                .AnyAsync(
                    s => s.Id == sessionId,
                    cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to check session existence {SessionId}",
                sessionId);
            return Errors.Infrastructure.DatabaseError(
                "ExistsSession", ex.Message);
        }
    }

}