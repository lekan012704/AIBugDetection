namespace Domain.Application.Dtos.BugDetection;

public sealed class UserAnswerDto
{
    public int QuestionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? RelatedPrinciple { get; set; }
}