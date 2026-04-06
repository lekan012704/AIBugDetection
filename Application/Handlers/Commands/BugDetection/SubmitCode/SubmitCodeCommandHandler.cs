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
using System.Text;
using System.Text.Json;

namespace Application.Handlers.Commands.BugDetection.SubmitCode;

internal sealed class SubmitCodeCommandHandler(
    IAiBugDetectionService aiService,
    ICodeAnalysisSessionRepository sessionRepository,
    IGitHubService gitHubService,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    ILogger<SubmitCodeCommandHandler> logger)
    : ICommandHandler<SubmitCodeCommand, InitialAnalysisResponse>
{
    public async Task<ErrorOr<InitialAnalysisResponse>> Handle(
        SubmitCodeCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var keycloakId = userContext.IdentityId;
            if (string.IsNullOrWhiteSpace(keycloakId))
                return Errors.Common.Unauthorized(
                    "User",
                    "Please login to analyze code");

            var codeResult = await ExtractCodeAsync(
                command, cancellationToken);
            if (codeResult.IsError)
                return codeResult.Errors;

            var (code, fileName) = codeResult.Value;

            if (string.IsNullOrWhiteSpace(code))
                return Errors.Common.Validation(
                    "Code",
                    "No code provided for analysis");

            if (code.Length > 100_000)
                return Errors.Common.Validation(
                    "Code",
                    "Code is too large. Maximum 100,000 characters.");

            var session = new CodeAnalysisSession
            {
                Id = Guid.NewGuid(),
                UserId = keycloakId,
                SubmissionType = command.SubmissionType,
                FileName = fileName,
                GitHubUrl = command.GitHubUrl,
                RawCode = code,
                DetectedLanguage = command.Language,
                Status = "InitialAnalysis",
                AiProvider = "Pending",
                FallbackUsed = false,
                TotalIssuesFound = 0,
                CreatedBy = userContext.UserName
            };

            await sessionRepository.AddAsync(
                session, cancellationToken);
            await unitOfWork.SaveChangesAsync(
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Session {SessionId} created. Running initial scan...",
                session.Id);

            // Step 5: Run initial AI scan
            var analysisResult = await aiService
                .RunInitialAnalysisAsync(
                    code, fileName, cancellationToken);

            if (analysisResult.IsError)
            {
                session.Status = "Failed";
                await unitOfWork.SaveChangesAsync(
                    cancellationToken: cancellationToken);
                return analysisResult.Errors;
            }

            var analysis = analysisResult.Value;

            // Step 6: Save initial results to session
            session.Status = "WaitingForContext";
            session.AiProvider = analysis.AiProvider;
            session.FallbackUsed = analysis.FallbackUsed;
            session.DetectedLanguage = analysis.DetectedContext.Language;
            session.DetectedFramework = analysis.DetectedContext.Framework;
            session.DetectedArchitecture = analysis.DetectedContext.Framework;
            session.DetectedPatterns = JsonSerializer.Serialize(
                analysis.DetectedContext.SuspectedPatterns);
            session.DetectedPrinciples = JsonSerializer.Serialize(
                analysis.DetectedContext.SuspectedPrinciples);
            session.InjectedServices = JsonSerializer.Serialize(
                analysis.DetectedContext.InjectedServices);
            session.CluesFound = JsonSerializer.Serialize(
                analysis.DetectedContext.CluesFound);
            session.InitialObservations = analysis.InitialObservations;
            session.FollowUpQuestions = JsonSerializer.Serialize(
                analysis.FollowUpQuestions);
            session.InitialAnalyzedAt = DateTime.UtcNow;

            // Save conversation
            session.Conversations.Add(new AnalysisConversation
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = "AI",
                Message = analysis.InitialObservations,
                Phase = "Initial",
                OrderIndex = 1,
                CreatedBy = "System"
            });

            await unitOfWork.PersistChangesAsync(
                logAuditTrail: true,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Initial scan complete. SessionId: {SessionId} " +
                "Questions: {Count} Provider: {Provider}",
                session.Id,
                analysis.FollowUpQuestions.Count,
                analysis.AiProvider);

            // Step 7: Return response
            return new InitialAnalysisResponse
            {
                SessionId = session.Id,
                Status = session.Status,
                DetectedContext = analysis.DetectedContext,
                InitialObservations = analysis.InitialObservations,
                FollowUpQuestions = analysis.FollowUpQuestions,
                AiProvider = analysis.AiProvider,
                FallbackUsed = analysis.FallbackUsed,
                CreatedAt = session.CreatedAt 
            };
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "Error during initial code scan");
            return Errors.Infrastructure.DatabaseError(
                "SubmitCode.Failed", ex.ToString());
        }
    }

    private async Task<ErrorOr<(string code, string? fileName)>>
        ExtractCodeAsync(
            SubmitCodeCommand command,
            CancellationToken cancellationToken)
    {
        switch (command.SubmissionType.ToLower())
        {
            case "snippet":
                if (string.IsNullOrWhiteSpace(command.CodeSnippet))
                    return Errors.Common.Validation(
                        "CodeSnippet",
                        "Code snippet cannot be empty");
                return (command.CodeSnippet, null);

            case "file":
                if (command.File is null)
                    return Errors.Common.Validation(
                        "File", "No file uploaded");
                using (var reader = new StreamReader(
                    command.File.OpenReadStream()))
                {
                    var code = await reader.ReadToEndAsync();
                    return (code, command.File.FileName);
                }

            case "githuburl":
                if (string.IsNullOrWhiteSpace(command.GitHubUrl))
                    return Errors.Common.Validation(
                        "GitHubUrl", "GitHub URL cannot be empty");
                var githubResult = await gitHubService
                    .FetchCodeAsync(
                        command.GitHubUrl, cancellationToken);
                if (githubResult.IsError)
                    return githubResult.Errors;
                return (githubResult.Value, command.GitHubUrl);

            case "multiplefiles":
                if (command.Files is null || !command.Files.Any())
                    return Errors.Common.Validation(
                        "Files", "No files uploaded");
                var sb = new StringBuilder();
                foreach (var file in command.Files)
                {
                    using var reader = new StreamReader(
                        file.OpenReadStream());
                    var fileCode = await reader.ReadToEndAsync();
                    sb.AppendLine(
                        $"// ===== File: {file.FileName} =====");
                    sb.AppendLine(fileCode);
                    sb.AppendLine();
                }
                return (sb.ToString(),
                    string.Join(", ",
                        command.Files.Select(f => f.FileName)));

            default:
                return Errors.Common.Validation(
                    "SubmissionType",
                    "Invalid type. Use: Snippet, File, GitHubUrl, MultipleFiles");
        }
    }
}