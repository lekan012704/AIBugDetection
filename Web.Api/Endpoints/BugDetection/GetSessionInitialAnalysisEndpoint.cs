using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Queries.BugDetection
    .GetSessionInitialAnalysis;
using Asp.Versioning.Conventions;
using Domain.Application.Dtos.BugDetection;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class GetSessionInitialAnalysisEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "api/v{version:apiVersion}/analysis/{sessionId:guid}/initial",
            async (
                Guid sessionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSessionInitialAnalysisQuery(
                    sessionId);

                var result = await sender
                    .Send(query, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(data),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags(Tags.BugDetection)
        .WithName("GetSessionInitialAnalysis")
        .RequireAuthorization()
        .Produces<InitialAnalysisResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}