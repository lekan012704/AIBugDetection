using Application.Abstractions.ExceptionHandlers;
using Application.Handlers.Commands.BugDetection.SubmitCode;
using Asp.Versioning.Conventions;
using Domain.Application.Dtos.BugDetection;
using MediatR;
using Microsoft.AspNetCore.Http;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.BugDetection;

internal sealed class SubmitCodeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v{version:apiVersion}/analysis/submit",
            async (
                HttpContext httpContext,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                // ✅ Read form data from HttpContext directly
                var form = await httpContext.Request
                    .ReadFormAsync(cancellationToken);

                var command = new SubmitCodeCommand(
                    CodeSnippet: form["codeSnippet"].ToString(),
                    File: form.Files.GetFile("file"),
                    GitHubUrl: form["gitHubUrl"].ToString(),
                    Files: form.Files
                        .GetFiles("files")
                        .ToList(),
                    Language: form["language"].ToString(),
                    SubmissionType: form["submissionType"]
                        .ToString());

                var result = await sender
                    .Send(command, cancellationToken)
                    .ConfigureAwait(false);

                return result.MatchWithValue(
                    onSuccess: data => Results.Ok(data),
                    onError: _ => CustomResults.Problem(result));
            })
        .WithTags("BugDetection")
        .WithName("SubmitCode")
        .RequireAuthorization()
        .DisableAntiforgery()

        .Accepts<IFormFile>("multipart/form-data")

        .Produces<InitialAnalysisResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithApiVersionSet(app.NewApiVersionSet()
            .HasApiVersion(ApiVersions.V1)
            .Build())
        .MapToApiVersion(ApiVersions.V1);
    }
}