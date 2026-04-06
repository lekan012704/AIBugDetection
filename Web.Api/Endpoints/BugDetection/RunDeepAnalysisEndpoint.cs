using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.BugDetection.RunDeepAnalysis;
using Asp.Versioning.Conventions;
using Domain.Application.Dtos.BugDetection;
using MediatR;
using Microsoft.AspNetCore.Http;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class RunDeepAnalysisEndpoint : IEndpoint
{
    public sealed record Request(
        Guid SessionId,
        List<UserAnswerDto> Answers);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/analysis/deep",
            async (
                Request request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new RunDeepAnalysisCommand(
                    request.SessionId,
                    request.Answers);

                var result = await sender
                    .Send(command, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(data),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags("BugDetection")
        .WithName("RunDeepAnalysis")
        .RequireAuthorization()
        .Produces<DeepAnalysisResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}