namespace Domain.Application.Dtos.BugDetection;

public sealed class FollowUpQuestion
{
    public int QuestionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public int? RelatedCodeLine { get; set; }
    public string? RelatedPrinciple { get; set; }
    public string Priority { get; set; } = string.Empty;
}