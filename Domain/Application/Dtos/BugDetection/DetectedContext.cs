namespace Domain.Application.Dtos.BugDetection;

public sealed class DetectedContext
{
    public string Language { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string CodeComplexity { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public List<ClueFound> CluesFound { get; set; } = new();
    public List<string> SuspectedPatterns { get; set; } = new();
    public List<string> SuspectedPrinciples { get; set; } = new();
    public List<InjectedServiceInfo> InjectedServices { get; set; } = new();
}