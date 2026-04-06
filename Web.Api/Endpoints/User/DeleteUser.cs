using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.Users.DeleteUser;
using Asp.Versioning.Conventions;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class DeleteUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v{version:apiVersion}/users/{userId}",
            async (
                string userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteUserCommand(userId);

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
        .WithName("DeleteUser")
        .RequireAuthorization()  // ✅ Must be logged in as admin
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