namespace Domain.Application.Entities.Outbox
{
    public class OutboxMessageFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
    }
}
