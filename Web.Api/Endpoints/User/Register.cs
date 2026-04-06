using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.Users.Register;
using Asp.Versioning.Conventions;
using ErrorOr;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/users/register", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            ErrorOr<string> result = await sender.Send(command, cancellationToken);

            return result.MatchWithValue(
                onSuccess: id => Results.Ok(id),
                onError: _ => CustomResults.Problem(result));

            //When your handler return just a success with any value to display
            //return result.MatchWithoutValue(
            //    onSuccess: () => Results.Ok(),
            //    onError: _ => CustomResults.Problem(result));
        })
        .RequireAuthorization()
        .WithName("RegisterUser")
        .WithTags(Tags.Users)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(ApiVersions.V1).Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}
