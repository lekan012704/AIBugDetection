namespace Domain.Application.Entities.Outbox
{
    public sealed class OutboxOptions
    {
        public int IntervalInSeconds { get; init; }
        public int IntervalInMinutes { get; init; }
        public int IntervalInHour { get; init; }
        public int BatchSize { get; init; }
        public int MaxRetryCount { get; init; }
    }
}
