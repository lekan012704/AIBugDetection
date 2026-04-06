using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;

namespace Application.Handlers.Commands.BugDetection.RunDeepAnalysis;

public sealed record RunDeepAnalysisCommand(
    Guid SessionId,
    List<UserAnswerDto> Answers
) : ICommand<DeepAnalysisResponse>;