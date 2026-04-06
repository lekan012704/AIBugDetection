namespace Domain.Application.Dtos.BugDetection;

public sealed class SessionSummaryResponse
{
    public Guid SessionId { get; set; }
    public string SubmissionType { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? GitHubUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DetectedLanguage { get; set; }
    public string? DetectedArchitecture { get; set; }
    public string? OverallCodeQuality { get; set; }
    public int TotalIssuesFound { get; set; }
    public int CriticalIssues { get; set; } 
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }
    public string AiProvider { get; set; } = string.Empty;
    public bool FallbackUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalAnalyzedAt { get; set; }
}