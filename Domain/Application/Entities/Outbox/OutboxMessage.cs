using SharedKernel;

namespace Domain.Application.Entities.Outbox
{
    public sealed class OutboxMessage : Entity<Guid>
    {
        public OutboxMessage(Guid id, DateTime createdOnUtc, string type, string content, string status, int retryCount, DateTime? updatedOn, string? lastUpdatedBy, string? error)
        {
            Id = id;
            CreatedAt = createdOnUtc;
            Content = content;
            Type = type;
            Status = status;
            RetryCount = retryCount;
            UpdatedAt = updatedOn;
            UpdatedBy = lastUpdatedBy;
            Error = error;
        }
        public OutboxMessage()
        {

        }
        public string Status { get; init; }

        public string Type { get; init; }

        public int RetryCount { get; init; }

        public string Content { get; init; }

        public DateTime? LastProcessedAtUtc { get; init; }

        public string? Error { get; init; }
    }
}
