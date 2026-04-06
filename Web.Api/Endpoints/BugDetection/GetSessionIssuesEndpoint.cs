using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Queries.BugDetection.GetSessionIssues;
using Asp.Versioning.Conventions;
using Domain.Application.Dtos.BugDetection;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class GetSessionIssuesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "api/v{version:apiVersion}/analysis/{sessionId:guid}/issues",
            async (
                Guid sessionId,
                ISender sender,
                CancellationToken cancellationToken,
                string? severity = null,
                string? issueType = null,
                string? phase = null) =>
            {
                var query = new GetSessionIssuesQuery(
                    sessionId,
                    severity,
                    issueType,
                    phase);

                var result = await sender
                    .Send(query, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(data),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags(Tags.BugDetection)
        .WithName("GetSessionIssues")
        .RequireAuthorization()
        .Produces<List<CodeIssueResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}