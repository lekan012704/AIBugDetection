namespace Domain.Application.Dtos.BugDetection;

public sealed class ServiceInjectionIssueDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string PrincipleViolated { get; set; } = string.Empty;
    public string SuggestedFix { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
}