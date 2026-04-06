using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.Users.ForgotPassword;
using Asp.Versioning.Conventions;
using MediatR;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class ForgotPassword : IEndpoint
{
    public sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/users/forgot-password",
            async (
                Request request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ForgotPasswordCommand(request.Email);

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
        .WithName("ForgotPassword")
        .AllowAnonymous()  
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}