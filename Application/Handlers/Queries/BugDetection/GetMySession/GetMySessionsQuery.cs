using Application.Abstractions.Messaging;
using Domain.Application.Dtos;
using Domain.Application.Dtos.BugDetection;

namespace Application.Handlers.Queries.BugDetection.GetMySessions;

public sealed record GetMySessionsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedResult<SessionSummaryResponse>>;