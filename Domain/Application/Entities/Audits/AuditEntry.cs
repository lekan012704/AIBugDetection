using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Domain.Application.Entities.Audits
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry, string actionByWho, string? tableName)
        {
            Entry = entry;
            ActionByWho = actionByWho;
            TableName = tableName;
        }

        public EntityEntry Entry { get; }
        private string ActionByWho { get; set; }
        private string? TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new List<string>();

        public Audit ToAudit()
        {
            var audit = Audit.CreateAudit(ActionByWho, AuditType.ToString(), TableName, DateTime.UtcNow,
                JsonConvert.SerializeObject(KeyValues),
                OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns));
            return audit;
        }
    }
}
