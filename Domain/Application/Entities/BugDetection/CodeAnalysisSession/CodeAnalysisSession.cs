using Domain.Application.Entities.Audits;
using Domain.Application.Entities.Users;
using SharedKernel;

namespace Domain.Application.Entities.BugDetection;

public sealed class CodeAnalysisSession :Entity<Guid>
{

    public required string UserId { get; set; }
    public User User { get; set; } = null!;
    public required string SubmissionType { get; set; }
    public string? FileName { get; set; }
    public string? GitHubUrl { get; set; }
    public required string RawCode { get; set; }
    public string? DetectedLanguage { get; set; }
    public string? DetectedFramework { get; set; }
    public string? DetectedArchitecture { get; set; }
    public string? DetectedPatterns { get; set; }
    public string? DetectedPrinciples { get; set; }
    public string? InjectedServices { get; set; }
    public string? CluesFound { get; set; }

    // Session state
    public required string Status { get; set; }
    // Pending → InitialAnalysis → WaitingForContext → DeepAnalysis → Completed → Failed

    // AI Provider
    public required string AiProvider { get; set; }
    public bool FallbackUsed { get; set; }

    // Phase 1 results
    public string? InitialObservations { get; set; }
    public string? FollowUpQuestions { get; set; }

    // Phase 2 inputs
    public string? UserAnswers { get; set; }

    // Phase 2 results
    public string? FinalAnalysis { get; set; }
    public string? PatternCompliance { get; set; }
    public string? ExecutiveSummary { get; set; }
    public string? ArchitectureAssessment { get; set; }
    public string? OverallCodeQuality { get; set; }
    public string? RefactoringPriorities { get; set; }
    public string? QuickWins { get; set; }
    public string? LongTermRecommendations { get; set; }

    // Stats
    public int TotalIssuesFound { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }

    // Timestamps
    public DateTime? InitialAnalyzedAt { get; set; }
    public DateTime? FinalAnalyzedAt { get; set; }

    // Navigation
    public ICollection<CodeIssue> Issues { get; set; }
        = new HashSet<CodeIssue>();
    public ICollection<AnalysisConversation> Conversations { get; set; }
        = new HashSet<AnalysisConversation>();
}