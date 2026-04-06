using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;

namespace Application.Handlers.Queries.BugDetection.GetSession;

public sealed record GetSessionQuery(
    Guid SessionId
) : IQuery<DeepAnalysisResponse>;