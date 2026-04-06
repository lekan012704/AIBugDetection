using Domain.Application.Entities.Audits;
using SharedKernel;

namespace Domain.Application.Entities.BugDetection;

public sealed class BugItem : Entity<Guid>
{
    public Guid BugReportId { get; set; }
    public BugReport BugReport { get; set; } = null!;
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Severity { get; set; }
    public int? LineNumber { get; set; }
    public int? EndLineNumber { get; set; }
    public string? CodeSnippet { get; set; }
    public required string SuggestedFix { get; set; }
    public string? FixedCode { get; set; }
    public string? Explanation { get; set; }
    public string? Category { get; set; }
    public int OrderIndex { get; set; }
}