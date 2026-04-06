using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Text.Json;

namespace Application.Handlers.Queries.BugDetection.GetSession;

internal sealed class GetSessionQueryHandler(
    ICodeAnalysisSessionRepository sessionRepository,
    IUserContext userContext,
    ILogger<GetSessionQueryHandler> logger)
    : IQueryHandler<GetSessionQuery, DeepAnalysisResponse>
{
    public async Task<ErrorOr<DeepAnalysisResponse>> Handle(
        GetSessionQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            if (query.SessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            var sessionResult = await sessionRepository
                .GetByIdWithDetailsAsync(
                    query.SessionId,
                    cancellationToken);

            if (sessionResult.IsError)
                return sessionResult.Errors;

            var session = sessionResult.Value;

            var keycloakId = userContext.IdentityId;
            if (session.UserId != keycloakId)
                return Errors.Common.Unauthorized(
                    "Session",
                    "You do not have access to this session");

            logger.LogInformation(
                "Retrieved session {SessionId}",
                query.SessionId);

            return MapToResponse(session);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error retrieving session {SessionId}",
                query.SessionId);
            return Errors.Infrastructure.DatabaseError(
                "GetSession.Failed", ex.ToString());
        }
    }

    private static DeepAnalysisResponse MapToResponse(
        Domain.Application.Entities.BugDetection.CodeAnalysisSession session)
    {
        var issues = session.Issues.ToList();

        return new DeepAnalysisResponse
        {
            SessionId = session.Id,
            Status = session.Status,
            DetectedContext = new DetectedContext
            {
                Language = session.DetectedLanguage ?? string.Empty,
                Framework = session.DetectedFramework ?? string.Empty,
                Architecture = session.DetectedArchitecture ?? string.Empty,
                SuspectedPatterns = Deserialize<List<string>>(
                    session.DetectedPatterns),
                SuspectedPrinciples = Deserialize<List<string>>(
                    session.DetectedPrinciples),
                InjectedServices = Deserialize<List<InjectedServiceInfo>>(
                    session.InjectedServices),
                CluesFound = Deserialize<List<ClueFound>>(
                    session.CluesFound)
            },
            ExecutiveSummary = session.ExecutiveSummary ?? string.Empty,
            ArchitectureAssessment = session.ArchitectureAssessment
                ?? string.Empty,
            OverallCodeQuality = session.OverallCodeQuality ?? string.Empty,
            PatternCompliance = Deserialize<List<PatternComplianceDto>>(
                session.PatternCompliance),
            SolidViolations = FilterIssues(issues, "SolidViolation"),
            ArchitectureViolations = FilterIssues(
                issues, "ArchitectureViolation"),
            PatternMisuses = FilterIssues(issues, "PatternMisuse"),
            DryViolations = FilterIssues(issues, "DryViolation"),
            KissViolations = FilterIssues(issues, "KissViolation"),
            YagniViolations = FilterIssues(issues, "YagniViolation"),
            AsyncViolations = FilterIssues(issues, "AsyncViolation"),
            DiViolations = FilterIssues(issues, "DiViolation"),
            Bugs = FilterIssues(issues, "Bug"),
            SecurityIssues = FilterIssues(issues, "SecurityIssue"),
            PerformanceIssues = FilterIssues(issues, "PerformanceIssue"),
            BusinessLogicIssues = FilterIssues(
                issues, "BusinessLogicIssue"),
            TotalIssuesFound = session.TotalIssuesFound,
            CriticalIssues = session.CriticalIssues,
            HighIssues = session.HighIssues,
            MediumIssues = session.MediumIssues,
            LowIssues = session.LowIssues,
            RefactoringPriorities = Deserialize<List<string>>(
                session.RefactoringPriorities),
            QuickWins = Deserialize<List<string>>(
                session.QuickWins),
            LongTermRecommendations = Deserialize<List<string>>(
                session.LongTermRecommendations),
            AiProvider = session.AiProvider,
            FallbackUsed = session.FallbackUsed,
            AnalyzedAt = session.FinalAnalyzedAt ?? DateTime.UtcNow
        };
    }

    private static List<CodeIssueResponse> FilterIssues(
        List<Domain.Application.Entities.BugDetection.CodeIssue> issues,
        string issueType) =>
        issues
            .Where(i => i.IssueType == issueType)
            .OrderBy(i => i.OrderIndex)
            .Select(i => new CodeIssueResponse
            {
                Id = i.Id,
                IssueType = i.IssueType,
                Title = i.Title,
                Description = i.Description,
                Severity = i.Severity,
                LineNumber = i.LineNumber,
                EndLineNumber = i.EndLineNumber,
                CodeSnippet = i.CodeSnippet,
                PatternViolated = i.PatternViolated,
                PrincipleViolated = i.PrincipleViolated,
                ArchitectureLayerViolated = i.ArchitectureLayerViolated,
                DeveloperIntent = i.DeveloperIntent,
                Contradiction = i.Contradiction,
                SuggestedFix = i.SuggestedFix,
                FixedCode = i.FixedCode,
                Explanation = i.Explanation,
                RefactoringSteps = i.RefactoringSteps,
                FoundInPhase = i.FoundInPhase
            }).ToList();

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