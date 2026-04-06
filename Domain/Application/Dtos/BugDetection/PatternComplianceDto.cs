namespace Domain.Application.Dtos.BugDetection;

public sealed class PatternComplianceDto
{
    public string Pattern { get; set; } = string.Empty;
    public bool DeveloperIntended { get; set; }
    public int ComplianceScore { get; set; }
    public string Summary { get; set; } = string.Empty;
}