using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Handlers.Queries.BugDetection.GetSessionIssues;

internal sealed class GetSessionIssuesQueryHandler(
    ICodeAnalysisSessionRepository sessionRepository,
    IUserContext userContext,
    ILogger<GetSessionIssuesQueryHandler> logger)
    : IQueryHandler<GetSessionIssuesQuery, List<CodeIssueResponse>>
{
    public async Task<ErrorOr<List<CodeIssueResponse>>> Handle(
        GetSessionIssuesQuery query,
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

            // ✅ Filter issues
            var issues = session.Issues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Severity))
                issues = issues.Where(i =>
                    i.Severity.ToLower() ==
                    query.Severity.ToLower());

            if (!string.IsNullOrWhiteSpace(query.IssueType))
                issues = issues.Where(i =>
                    i.IssueType.ToLower() ==
                    query.IssueType.ToLower());

            if (!string.IsNullOrWhiteSpace(query.Phase))
                issues = issues.Where(i =>
                    i.FoundInPhase.ToLower() ==
                    query.Phase.ToLower());

            // ✅ Extract the ordering to a local function first
            static int GetSeverityOrder(string severity) => severity switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };

            var result = issues
                .ToList()  
                .OrderByDescending(i => GetSeverityOrder(i.Severity))
                .ThenBy(i => i.OrderIndex)
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
                })
                .ToList();

            logger.LogInformation(
                "Retrieved {Count} issues for session {SessionId}",
                result.Count, query.SessionId);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error retrieving issues for {SessionId}",
                query.SessionId);
            return Errors.Infrastructure.DatabaseError(
                "GetSessionIssues.Failed", ex.ToString());
        }
    }
}