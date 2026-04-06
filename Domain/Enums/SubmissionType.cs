namespace Domain.Application.Enums;

public enum SubmissionType
{
    Snippet,
    File,
    GitHubUrl,
    MultipleFiles
}

public enum SessionStatus
{
    Pending,
    InitialAnalysis,
    WaitingForContext,
    DeepAnalysis,
    Completed,
    Failed
}

public enum IssueType
{
    Bug,
    ArchitectureViolation,
    SolidViolation,
    PatternMisuse,
    DryViolation,
    KissViolation,
    YagniViolation,
    AsyncViolation,
    DiViolation,
    SecurityIssue,
    PerformanceIssue,
    BusinessLogicIssue,
    ServiceInjectionIssue,
    DesignSmell
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AiProvider
{
    Claude,
    OpenAI
}

public enum AnalysisPhase
{
    Initial,
    Deep
}

public enum ConversationRole
{
    AI,
    User
}

public enum CodeQuality
{
    Poor,
    NeedsWork,
    Good,
    Excellent
}