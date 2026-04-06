using Microsoft.AspNetCore.Http;

namespace Domain.Application.Dtos.BugDetection;

public sealed class InitialAnalysisRequest
{
    public string? CodeSnippet { get; set; }
    public IFormFile? File { get; set; }
    public string? GitHubUrl { get; set; }
    public List<IFormFile>? Files { get; set; }
    public string? Language { get; set; }
    public required string SubmissionType { get; set; }
}