using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;

namespace Application.Handlers.Queries.BugDetection.GetSessionInitialAnalysis;

public sealed record GetSessionInitialAnalysisQuery(
    Guid SessionId
) : IQuery<InitialAnalysisResponse>;