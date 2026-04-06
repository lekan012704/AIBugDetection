using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Text.Json;

namespace Application.Handlers.Queries.BugDetection.GetSessionInitialAnalysis;

internal sealed class GetSessionInitialAnalysisQueryHandler(
    ICodeAnalysisSessionRepository sessionRepository,
    IUserContext userContext,
    ILogger<GetSessionInitialAnalysisQueryHandler> logger)
    : IQueryHandler<GetSessionInitialAnalysisQuery, InitialAnalysisResponse>
{
    public async Task<ErrorOr<InitialAnalysisResponse>> Handle(
        GetSessionInitialAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            if (query.SessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            var session = await sessionRepository
                .GetByIdAsync(
                    query.SessionId,
                    cancellationToken);

            if (session is null)
                return Errors.Common.NotFound(
                    "Session",
                    query.SessionId.ToString());

            var keycloakId = userContext.IdentityId;
            if (session.UserId != keycloakId)
                return Errors.Common.Unauthorized(
                    "Session",
                    "You do not have access to this session");

            logger.LogInformation(
                "Retrieved initial analysis for session {SessionId}",
                query.SessionId);

            return new InitialAnalysisResponse
            {
                SessionId = session.Id,
                Status = session.Status,
                DetectedContext = new DetectedContext
                {
                    Language = session.DetectedLanguage ?? string.Empty,
                    Framework = session.DetectedFramework ?? string.Empty,
                    Architecture = session.DetectedArchitecture
                        ?? string.Empty,
                    SuspectedPatterns = Deserialize<List<string>>(
                        session.DetectedPatterns),
                    SuspectedPrinciples = Deserialize<List<string>>(
                        session.DetectedPrinciples),
                    InjectedServices = Deserialize<List<InjectedServiceInfo>>(
                        session.InjectedServices),
                    CluesFound = Deserialize<List<ClueFound>>(
                        session.CluesFound)
                },
                InitialObservations = session.InitialObservations
                    ?? string.Empty,
                FollowUpQuestions = Deserialize<List<FollowUpQuestion>>(
                    session.FollowUpQuestions),
                AiProvider = session.AiProvider,
                FallbackUsed = session.FallbackUsed,
                CreatedAt = session.CreatedAt 
            };
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error retrieving initial analysis {SessionId}",
                query.SessionId);
            return Errors.Infrastructure.DatabaseError(
                "GetInitialAnalysis.Failed", ex.ToString());
        }
    }

    private static T Deserialize<T>(string? json)
        where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new T();
        }
        catch
        {
            return new T();
        }
    }
}