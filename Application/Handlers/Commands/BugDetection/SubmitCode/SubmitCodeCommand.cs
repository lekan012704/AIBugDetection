using Application.Abstractions.Messaging;
using Domain.Application.Dtos.BugDetection;
using Microsoft.AspNetCore.Http;

namespace Application.Handlers.Commands.BugDetection.SubmitCode;

public sealed record SubmitCodeCommand(
    string? CodeSnippet,
    IFormFile? File,
    string? GitHubUrl,
    List<IFormFile>? Files,
    string? Language,
    string SubmissionType
) : ICommand<InitialAnalysisResponse>;  