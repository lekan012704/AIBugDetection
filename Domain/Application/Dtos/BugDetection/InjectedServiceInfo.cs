namespace Domain.Application.Dtos.BugDetection;

public sealed class InjectedServiceInfo
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsInterface { get; set; }
    public string? Concern { get; set; }
}