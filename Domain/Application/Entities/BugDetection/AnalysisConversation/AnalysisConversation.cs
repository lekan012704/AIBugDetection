using Domain.Application.Entities.Audits;
using SharedKernel;

namespace Domain.Application.Entities.BugDetection;

public sealed class AnalysisConversation : Entity<Guid>
{

    public Guid SessionId { get; set; }
    public CodeAnalysisSession Session { get; set; } = null!;

    public required string Role { get; set; } 
    public required string Message { get; set; }
    public required string Phase { get; set; } 
    public int OrderIndex { get; set; }
}