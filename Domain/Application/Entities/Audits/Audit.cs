using Domain.Application.Entities.Users;
using SharedKernel;

namespace Domain.Application.Entities.Audits
{
    public sealed class Audit : Entity<AuditId>
    {
        private Audit(AuditId id, string actionByWho,
            string type, string? tableName,
            DateTime dateTime, string oldValues,
            string? newValues, string? affectedColumns,
            string? primaryKey)
            : base(id)
        {
            ActionByWho = actionByWho ?? throw new ArgumentNullException(nameof(actionByWho));
            ActionType = type ?? throw new ArgumentNullException(nameof(type));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            DateTime = dateTime;
            OldValues = oldValues;
            NewValues = newValues ?? throw new ArgumentNullException(nameof(newValues));
            AffectedColumns = affectedColumns;
            PrimaryKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
        }

        private Audit()
        {
        }

        public User? User { get; set; }
        public string ActionByWho { get; private set; }
        public string ActionType { get; private set; }
        public string TableName { get; private set; }
        public DateTime DateTime { get; private set; }
        public string OldValues { get; private set; }
        public string NewValues { get; private set; }
        public string? AffectedColumns { get; private set; }
        public string PrimaryKey { get; private set; }

        public static Audit CreateAudit(
            string actionByWho, string type, string? tableName, DateTime dateTime, string oldValues,
            string? newValues, string? affectedColumns,
            string? primaryKey)
        {
            var audit = new Audit(
                AuditId.New(),
                actionByWho,
                type,
                tableName,
                dateTime,
                oldValues,
                newValues,
                affectedColumns,
                primaryKey
            )
            { CreatedBy = "System Admin" };
            return audit;
        }
    }
}
