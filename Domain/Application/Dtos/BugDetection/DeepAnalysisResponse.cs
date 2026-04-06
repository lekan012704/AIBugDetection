namespace Domain.Application.Dtos.BugDetection;

public sealed class DeepAnalysisResponse
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DetectedContext DetectedContext { get; set; } = new();
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string ArchitectureAssessment { get; set; } = string.Empty;
    public string OverallCodeQuality { get; set; } = string.Empty;

    public List<PatternComplianceDto> PatternCompliance { get; set; } = new();

    public List<CodeIssueResponse> SolidViolations { get; set; } = new();

    public List<CodeIssueResponse> ArchitectureViolations { get; set; } = new();
    public List<CodeIssueResponse> PatternMisuses { get; set; } = new();
    public List<CodeIssueResponse> DryViolations { get; set; } = new();
    public List<CodeIssueResponse> KissViolations { get; set; } = new();
    public List<CodeIssueResponse> YagniViolations { get; set; } = new();
    public List<CodeIssueResponse> AsyncViolations { get; set; } = new();
    public List<CodeIssueResponse> DiViolations { get; set; } = new();

    public List<CodeIssueResponse> Bugs { get; set; } = new();
    public List<CodeIssueResponse> SecurityIssues { get; set; } = new();
    public List<CodeIssueResponse> PerformanceIssues { get; set; } = new();
    public List<CodeIssueResponse> BusinessLogicIssues { get; set; } = new();

    public List<ServiceInjectionIssueDto> ServiceInjectionIssues { get; set; } = new();

    public int TotalIssuesFound { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }

    public List<string> RefactoringPriorities { get; set; } = new();
    public List<string> QuickWins { get; set; } = new();
    public List<string> LongTermRecommendations { get; set; } = new();

    public string AiProvider { get; set; } = string.Empty;
    public bool FallbackUsed { get; set; }
    public DateTime AnalyzedAt { get; set; }
}