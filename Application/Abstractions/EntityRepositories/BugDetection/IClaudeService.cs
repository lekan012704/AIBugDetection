using Domain.Application.Dtos.BugDetection;
using ErrorOr;

namespace Application.Abstractions.AI;

public interface IClaudeService
{
    Task<ErrorOr<AiInitialAnalysisResult>> RunInitialAnalysisAsync(
        string code,
        string? fileName,
        CancellationToken cancellationToken);

    Task<ErrorOr<AiDeepAnalysisResult>> RunDeepAnalysisAsync(
        string code,
        string initialObservations,
        string userAnswers,
        string? fileName,
        CancellationToken cancellationToken);
}