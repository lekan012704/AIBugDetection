using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.Users.UpdateUser;
using Asp.Versioning.Conventions;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class UpdateUser : IEndpoint
{
    public sealed record Request(
        string? FirstName,
        string? LastName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v{version:apiVersion}/users/update",
            async (
                Request request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateUserCommand(
                    request.FirstName,
                    request.LastName
                    );

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
        .WithTags(Tags.Users)
        .WithName("UpdateUser")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
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