using Domain.Application.Entities.Audits;
using SharedKernel;

namespace Domain.Application.Entities.BugDetection;

public sealed class CodeIssue : Entity<Guid>
{

    public Guid SessionId { get; set; }
    public CodeAnalysisSession Session { get; set; } = null!;

    // Classification
    public required string IssueType { get; set; }


    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Severity { get; set; }


    // Location
    public int? LineNumber { get; set; }
    public int? EndLineNumber { get; set; }
    public string? CodeSnippet { get; set; }

    // Context
    public string? PatternViolated { get; set; }
    public string? PrincipleViolated { get; set; }
    public string? ArchitectureLayerViolated { get; set; }
    public string? DeveloperIntent { get; set; }
    public string? Contradiction { get; set; }

    // Fix
    public required string SuggestedFix { get; set; }
    public string? FixedCode { get; set; }
    public string? Explanation { get; set; }
    public string? RefactoringSteps { get; set; }

    // Phase
    public required string FoundInPhase { get; set; }
 

    public int OrderIndex { get; set; }
    public bool IsConfirmed { get; set; } = true;
}