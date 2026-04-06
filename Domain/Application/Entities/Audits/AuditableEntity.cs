using SharedKernel;

namespace Domain.Application.Entities.Audits
{
    public abstract class   AuditableEntity : Entity<AuditId>
    {
        protected AuditableEntity(AuditId id, string createdBy, DateTime createdOn, string? modifiedBy, DateTime? modifiedOn)
            : base(id)
        {
            CreatedBy = createdBy;
            CreatedAt = createdOn;
            UpdatedBy = modifiedBy;
            UpdatedAt = modifiedOn;
        }

        private AuditableEntity()
        {
        }
    }
}
