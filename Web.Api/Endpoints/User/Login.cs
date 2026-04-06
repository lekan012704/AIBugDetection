using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.Users.Login;
using Asp.Versioning.Conventions;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    //called as => https://localhost:44360/api/v1/users/login
    public sealed record Request(string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/users/login", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
            return result.MatchWithValue(
                onSuccess: data => Results.Ok(data),
                onError: _ => CustomResults.Problem(result));
        })
        .WithTags(Tags.Users)
        .WithName("Login")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(ApiVersions.V1).Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}
