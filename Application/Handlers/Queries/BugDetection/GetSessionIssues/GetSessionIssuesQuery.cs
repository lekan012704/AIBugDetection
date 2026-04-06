using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;

namespace Application.Handlers.Queries.BugDetection.GetSessionIssues;

public sealed record GetSessionIssuesQuery(
    Guid SessionId,
    string? Severity,
    string? IssueType,
    string? Phase
) : IQuery<List<CodeIssueResponse>>;