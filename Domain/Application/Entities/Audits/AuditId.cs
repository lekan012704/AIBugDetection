namespace Domain.Application.Entities.Audits
{
    public record AuditId(Guid Value)
    {
        public static AuditId New() => new(Guid.NewGuid());
    }
}
