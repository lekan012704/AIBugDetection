using Application.Abstractions.AI;
using Domain.Application.Dtos.BugDetection;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Services;

public sealed class AiBugDetectionService(
    IClaudeService claudeService,
    IOpenAiService openAiService,
    ILogger<AiBugDetectionService> logger)
    : IAiBugDetectionService
{
    public async Task<ErrorOr<AiInitialAnalysisResult>> RunInitialAnalysisAsync(
        string code,
        string? fileName,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Running initial analysis with OpenAI...");

        //var claudeResult = await claudeService
        //    .RunInitialAnalysisAsync(code, fileName, cancellationToken);

        //if (!claudeResult.IsError)
        //{
        //    logger.LogInformation(
        //        "Claude initial analysis successful");
        //    return claudeResult;
        //}
        //logger.LogWarning(
        //    "Claude failed for initial analysis. " +
        //    "Falling back to OpenAI. Error: {Error}",
        //    claudeResult.Errors.First().Description);

        var openAiResult = await openAiService
            .RunInitialAnalysisAsync(code, fileName, cancellationToken);

        if (!openAiResult.IsError)
        {
            logger.LogInformation(
                "OpenAI fallback initial analysis successful");
            openAiResult.Value.FallbackUsed = true;
            return openAiResult;
        }

        //logger.LogError(
        //    "Both AI providers failed for initial analysis. " +
        //    "Claude: {Claude} OpenAI: {OpenAI}",
        //    claudeResult.Errors.First().Description,
        //    openAiResult.Errors.First().Description);

        return Errors.Infrastructure.DatabaseError(
            "AI.BothFailed",
            "Both AI providers failed. Please try again.");
    }

    public async Task<ErrorOr<AiDeepAnalysisResult>> RunDeepAnalysisAsync(
        string code,
        string initialObservations,
        string userAnswers,
        string? fileName,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Running deep analysis with OpenAi...");

        //var claudeResult = await claudeService.RunDeepAnalysisAsync(
        //    code, initialObservations, userAnswers,
        //    fileName, cancellationToken);

        //if (!claudeResult.IsError)
        //{
        //    logger.LogInformation(
        //        "Claude deep analysis successful");
        //    return claudeResult;
        //}
        //logger.LogWarning(
        //    "Claude failed for deep analysis. " +
        //    "Falling back to OpenAI. Error: {Error}",
        //    claudeResult.Errors.First().Description);

        var openAiResult = await openAiService.RunDeepAnalysisAsync(
            code, initialObservations, userAnswers,
            fileName, cancellationToken);

        if (!openAiResult.IsError)
        {
            logger.LogInformation(
                "OpenAI fallback deep analysis successful");
            openAiResult.Value.FallbackUsed = true;
            return openAiResult;
        }

        //logger.LogError(
        //    "Both AI providers failed for deep analysis. " +
        //    "Claude: {Claude} OpenAI: {OpenAI}",
        //    claudeResult.Errors.First().Description,
        //    openAiResult.Errors.First().Description);

        return Errors.Infrastructure.DatabaseError(
            "AI.BothFailed",
            "Both AI providers failed. Please try again.");
    }
}