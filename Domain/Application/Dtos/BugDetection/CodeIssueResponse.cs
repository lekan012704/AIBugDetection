namespace Domain.Application.Dtos.BugDetection;

public sealed class CodeIssueResponse
{
    public Guid Id { get; set; }
    public string IssueType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
    public int? EndLineNumber { get; set; }
    public string? CodeSnippet { get; set; }
    public string? PatternViolated { get; set; }
    public string? PrincipleViolated { get; set; }
    public string? ArchitectureLayerViolated { get; set; }
    public string? DeveloperIntent { get; set; }
    public string? Contradiction { get; set; }
    public string SuggestedFix { get; set; } = string.Empty;
    public string? FixedCode { get; set; }
    public string? Explanation { get; set; }
    public string? RefactoringSteps { get; set; }
    public string FoundInPhase { get; set; } = string.Empty;
}