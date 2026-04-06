using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Queries.BugDetection.GetMySessions;
using Asp.Versioning.Conventions;
using Domain.Application.Dtos;
using Domain.Application.Dtos.BugDetection;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class GetMySessionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "api/v{version:apiVersion}/analysis/my-sessions",
            async (
                ISender sender,
                CancellationToken cancellationToken,
                int pageNumber = 1,
                int pageSize = 10) =>
            {
                var query = new GetMySessionsQuery(
                    pageNumber,
                    pageSize);

                var result = await sender
                    .Send(query, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(data),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags(Tags.BugDetection)
        .WithName("GetMySessions")
        .RequireAuthorization()
        .Produces<PagedResult<SessionSummaryResponse>>(
            StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}