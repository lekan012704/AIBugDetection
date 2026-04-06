using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.BugDetection.DeleteSession;
using Asp.Versioning.Conventions;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class DeleteSessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "api/v{version:apiVersion}/analysis/{sessionId:guid}",
            async (
                Guid sessionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteSessionCommand(sessionId);

                var result = await sender
                    .Send(command, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(new
                    {
                        Message = data
                    }),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags(Tags.BugDetection)
        .WithName("DeleteSession")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}