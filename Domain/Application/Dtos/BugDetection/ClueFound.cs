namespace Domain.Application.Dtos.BugDetection;

public sealed class ClueFound
{
    public string Clue { get; set; } = string.Empty;
    public string RelatedPrinciple { get; set; } = string.Empty;
    public int? LineNumber { get; set; }
}