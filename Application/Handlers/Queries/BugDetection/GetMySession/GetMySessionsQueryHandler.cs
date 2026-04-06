using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Queries.BugDetection.GetMySessions;

internal sealed class GetMySessionsQueryHandler(
    ICodeAnalysisSessionRepository sessionRepository,
    IUserContext userContext,
    ILogger<GetMySessionsQueryHandler> logger)
    : IQueryHandler<GetMySessionsQuery, PagedResult<SessionSummaryResponse>>
{
    public async Task<ErrorOr<PagedResult<SessionSummaryResponse>>> Handle(
        GetMySessionsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var keycloakId = userContext.IdentityId;
            if (string.IsNullOrWhiteSpace(keycloakId))
                return Errors.Common.Unauthorized(
                    "User", "Please login");

            if (query.PageNumber < 1)
                return Errors.Common.Validation(
                    "PageNumber",
                    "Page number must be greater than 0");

            if (query.PageSize < 1)
                return Errors.Common.Validation(
                    "PageSize",
                    "Page size must be greater than 0");

            var summariesResult = await sessionRepository
                .GetSummariesByUserIdAsync(
                    keycloakId,
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken);

            if (summariesResult.IsError)
                return summariesResult.Errors;

            var countResult = await sessionRepository
                .GetTotalCountByUserIdAsync(
                    keycloakId,
                    cancellationToken);

            if (countResult.IsError)
                return countResult.Errors;

            logger.LogInformation(
                "Retrieved {Count} sessions for user {UserId}",
                summariesResult.Value.Count, keycloakId);

            return new PagedResult<SessionSummaryResponse>
            {
                Data = summariesResult.Value,
                TotalCount = countResult.Value,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(
                    countResult.Value / (double)query.PageSize)
            };
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error retrieving sessions");
            return Errors.Infrastructure.DatabaseError(
                "GetMySessions.Failed", ex.ToString());
        }
    }
}