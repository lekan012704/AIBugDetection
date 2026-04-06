using Domain.Application.Entities.Users;
using SharedKernel;

namespace Domain.Application.Entities.BugDetection;

public sealed class BugReport : Entity<Guid>
{
    public required string UserId { get; set; }
    public User User { get; set; } = null!;
    public required string SubmissionType { get; set; }
    public string? FileName { get; set; }
    public string? GitHubUrl { get; set; }
    public required string RawCode { get; set; }
    public string? ProgrammingLanguage { get; set; }
    public required string AiProvider { get; set; }
    public bool FallbackUsed { get; set; }
    public required string Status { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public int TotalBugsFound { get; set; }
    public string? Summary { get; set; }

    public ICollection<BugItem> BugItems { get; set; }
        = new HashSet<BugItem>();
}