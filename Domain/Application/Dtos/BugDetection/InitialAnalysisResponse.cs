namespace Domain.Application.Dtos.BugDetection;

public sealed class InitialAnalysisResponse
{
    public Guid SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DetectedContext DetectedContext { get; set; } = new();
    public string InitialObservations { get; set; } = string.Empty;
    public List<FollowUpQuestion> FollowUpQuestions { get; set; } = new();
    public string AiProvider { get; set; } = string.Empty;
    public bool FallbackUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}