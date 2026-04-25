using Application.Abstractions.AI;
using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;
using Domain.Application.Entities.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Text.Json;

namespace Application.Handlers.Commands.BugDetection.RunDeepAnalysis;

internal sealed class RunDeepAnalysisCommandHandler(
    IAiBugDetectionService aiService,
    ICodeAnalysisSessionRepository sessionRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    ILogger<RunDeepAnalysisCommandHandler> logger)
    : ICommandHandler<RunDeepAnalysisCommand, DeepAnalysisResponse>
{
    public async Task<ErrorOr<DeepAnalysisResponse>> Handle(
        RunDeepAnalysisCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            if (command.SessionId == Guid.Empty)
                return Errors.Common.Validation(
                    "SessionId",
                    "SessionId cannot be empty");

            if (command.Answers is null || !command.Answers.Any())
                return Errors.Common.Validation(
                    "Answers",
                    "Please answer the follow-up questions");

            var sessionResult = await sessionRepository
                .GetByIdWithDetailsAsync(
                    command.SessionId, cancellationToken);

            if (sessionResult.IsError)
                return sessionResult.Errors;

            var session = sessionResult.Value;

            var keycloakId = userContext.IdentityId;
            if (session.UserId != keycloakId)
                return Errors.Common.Unauthorized(
                    "Session",
                    "You do not have access to this session");

            if (session.Status != "WaitingForContext")
                return Errors.Common.Validation(
                    "Session",
                    $"Session is '{session.Status}'. " +
                    "Expected 'WaitingForContext'");

            var answersJson = JsonSerializer.Serialize(command.Answers);
            session.UserAnswers = answersJson;
            session.Status = "DeepAnalysis";

            session.Conversations.Add(new AnalysisConversation
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = "User",
                Message = answersJson,
                Phase = "FollowUp",
                OrderIndex = session.Conversations.Count + 1,
                CreatedBy = userContext.UserName
            });

            await sessionRepository.UpdateAsync(session, cancellationToken);

            logger.LogInformation(
                "Starting deep analysis for session {SessionId}",       
                session.Id);

            // Step 6: Run deep AI analysis
            var deepResult = await aiService.RunDeepAnalysisAsync(
                session.RawCode,        
                session.InitialObservations ?? string.Empty,
                answersJson,
                session.FileName,
                cancellationToken);

            if (deepResult.IsError)
            {
                session.Status = "Failed";
                await sessionRepository.UpdateAsync(
                    session, cancellationToken);
                return deepResult.Errors;
            }

            var analysis = deepResult.Value;

            // Step 7: Update session with deep results
            session.Status = "Completed";
            session.AiProvider = analysis.AiProvider;
            session.FallbackUsed = analysis.FallbackUsed;
            session.ExecutiveSummary = analysis.ExecutiveSummary;
            session.ArchitectureAssessment = analysis.ArchitectureAssessment;
            session.OverallCodeQuality = analysis.OverallCodeQuality;
            session.PatternCompliance = JsonSerializer.Serialize(
                analysis.PatternCompliance);
            session.RefactoringPriorities = JsonSerializer.Serialize(
                analysis.RefactoringPriorities);
            session.QuickWins = JsonSerializer.Serialize(
                analysis.QuickWins);
            session.LongTermRecommendations = JsonSerializer.Serialize(
                analysis.LongTermRecommendations);
            session.FinalAnalyzedAt = DateTime.UtcNow;

            // Step 8: Collect and save all issues
            var allIssues = analysis.SolidViolations
                .Concat(analysis.ArchitectureViolations)
                .Concat(analysis.PatternMisuses)
                .Concat(analysis.DryViolations)
                .Concat(analysis.KissViolations)
                .Concat(analysis.YagniViolations)
                .Concat(analysis.AsyncViolations)
                .Concat(analysis.DiViolations)
                .Concat(analysis.Bugs)
                .Concat(analysis.SecurityIssues)
                .Concat(analysis.PerformanceIssues)
                .Concat(analysis.BusinessLogicIssues)
                .ToList();

            session.TotalIssuesFound = allIssues.Count;
            session.CriticalIssues = allIssues
                .Count(i => i.Severity == "Critical");
            session.HighIssues = allIssues
                .Count(i => i.Severity == "High");
            session.MediumIssues = allIssues
                .Count(i => i.Severity == "Medium");
            session.LowIssues = allIssues
                .Count(i => i.Severity == "Low");

            var codeIssues = allIssues
                .Select((issue, index) => new CodeIssue
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    IssueType = issue.IssueType,
                    Title = issue.Title,
                    Description = issue.Description,
                    Severity = issue.Severity,
                    LineNumber = issue.LineNumber,
                    EndLineNumber = issue.EndLineNumber,
                    CodeSnippet = issue.CodeSnippet,
                    PatternViolated = issue.PatternViolated,
                    PrincipleViolated = issue.PrincipleViolated,
                    ArchitectureLayerViolated =
                        issue.ArchitectureLayerViolated,
                    DeveloperIntent = issue.DeveloperIntent,
                    Contradiction = issue.Contradiction,
                    SuggestedFix = issue.SuggestedFix,
                    FixedCode = issue.FixedCode,
                    Explanation = issue.Explanation,
                    RefactoringSteps = issue.RefactoringSteps,
                    FoundInPhase = "Deep",
                    OrderIndex = session.Issues.Count + index + 1,
                    CreatedBy = userContext.UserName
                }).ToList();

            foreach (var issue in codeIssues)
                session.Issues.Add(issue);

            session.Conversations.Add(new AnalysisConversation
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = "AI",
                Message = analysis.ExecutiveSummary,
                Phase = "Deep",
                OrderIndex = session.Conversations.Count + 1,
                CreatedBy = "System"
            });

            await sessionRepository.UpdateAsync(session, cancellationToken);


            logger.LogInformation(
                "Deep analysis complete. SessionId: {SessionId} " +
                "Total: {Total} Critical: {Critical}",
                session.Id,
                session.TotalIssuesFound,
                session.CriticalIssues);

            return BuildDeepAnalysisResponse(session, analysis);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error during deep analysis for session {SessionId}",
                command.SessionId);
            return Errors.Infrastructure.DatabaseError(
                "DeepAnalysis.Failed", ex.ToString());
        }
    }

    private static DeepAnalysisResponse BuildDeepAnalysisResponse(
        CodeAnalysisSession session,
        AiDeepAnalysisResult analysis)
    {
        return new DeepAnalysisResponse
        {
            SessionId = session.Id,
            Status = session.Status,
            DetectedContext = new DetectedContext
            {
                Language = session.DetectedLanguage ?? string.Empty,
                Framework = session.DetectedFramework ?? string.Empty,
                Architecture = session.DetectedArchitecture
                    ?? string.Empty,
                SuspectedPatterns = JsonSerializer
                    .Deserialize<List<string>>(
                        session.DetectedPatterns ?? "[]") ?? new(),
                SuspectedPrinciples = JsonSerializer
                    .Deserialize<List<string>>(
                        session.DetectedPrinciples ?? "[]") ?? new(),
                InjectedServices = JsonSerializer
                    .Deserialize<List<InjectedServiceInfo>>(
                        session.InjectedServices ?? "[]") ?? new(),
                CluesFound = JsonSerializer
                    .Deserialize<List<ClueFound>>(
                        session.CluesFound ?? "[]") ?? new()
            },
            ExecutiveSummary = analysis.ExecutiveSummary,
            ArchitectureAssessment = analysis.ArchitectureAssessment,
            OverallCodeQuality = analysis.OverallCodeQuality,
            PatternCompliance = analysis.PatternCompliance,
            SolidViolations = analysis.SolidViolations,
            ArchitectureViolations = analysis.ArchitectureViolations,
            PatternMisuses = analysis.PatternMisuses,
            DryViolations = analysis.DryViolations,
            KissViolations = analysis.KissViolations,
            YagniViolations = analysis.YagniViolations,
            AsyncViolations = analysis.AsyncViolations,
            DiViolations = analysis.DiViolations,
            Bugs = analysis.Bugs,
            SecurityIssues = analysis.SecurityIssues,
            PerformanceIssues = analysis.PerformanceIssues,
            BusinessLogicIssues = analysis.BusinessLogicIssues,
            ServiceInjectionIssues = analysis.ServiceInjectionIssues,
            TotalIssuesFound = session.TotalIssuesFound,
            CriticalIssues = session.CriticalIssues,
            HighIssues = session.HighIssues,
            MediumIssues = session.MediumIssues,
            LowIssues = session.LowIssues,
            RefactoringPriorities = analysis.RefactoringPriorities,
            QuickWins = analysis.QuickWins,
            LongTermRecommendations = analysis.LongTermRecommendations,
            AiProvider = session.AiProvider,
            FallbackUsed = session.FallbackUsed,
            AnalyzedAt = session.FinalAnalyzedAt ?? DateTime.UtcNow
        };
    }
}