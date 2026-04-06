using Domain.Application.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Application.Entities.Configurations
{
    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.ToTable("AuditLog");

            builder.HasKey(audit => audit.Id);

            builder.Property(audit => audit.Id)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(audit => audit.Id)
                .HasConversion(audit => audit.Value, value => new AuditId(value));

            builder.Property(audit => audit.ActionByWho).HasMaxLength(450);
            builder.Property(audit => audit.ActionType).HasMaxLength(50);
            builder.Property(audit => audit.TableName).HasMaxLength(100);
            builder.Property(audit => audit.DateTime).IsRequired();
            builder.Property(audit => audit.OldValues).HasMaxLength(int.MaxValue);
            builder.Property(audit => audit.NewValues).HasMaxLength(int.MaxValue);
            builder.Property(audit => audit.AffectedColumns).HasMaxLength(1000);
            builder.Property(audit => audit.PrimaryKey).HasMaxLength(100);
            builder.Property(a => a.CreatedBy).HasDefaultValue("System Admin");

            // Configure property access mode for private setters
            // EF Core can work with private setters directly
            builder.UsePropertyAccessMode(PropertyAccessMode.Property);

            // Indexes for performance
            builder.HasIndex(a => a.ActionByWho)
                .HasDatabaseName("IX_Audits_ActionByWho");

            builder.HasIndex(a => a.TableName)
                .HasDatabaseName("IX_Audits_TableName");

            builder.HasIndex(a => a.ActionType)
                .HasDatabaseName("IX_Audits_ActionType");

            builder.HasIndex(a => a.DateTime)
                .HasDatabaseName("IX_Audits_DateTime");

            builder.HasIndex(a => a.PrimaryKey)
                .HasDatabaseName("IX_Audits_PrimaryKey");

            // Composite index for common queries
            builder.HasIndex(a => new { a.TableName, a.PrimaryKey, a.DateTime })
                .HasDatabaseName("IX_Audits_TableName_PrimaryKey_DateTime");

            builder.HasIndex(a => new { a.ActionByWho, a.DateTime })
                .HasDatabaseName("IX_Audits_ActionByWho_DateTime");

            // Relationships
            builder.HasOne(a => a.User)
                .WithMany() // Assuming User doesn't have a navigation property back to Audit
                .HasForeignKey(a => a.ActionByWho)
                .OnDelete(DeleteBehavior.SetNull) // Preserve audit records even if user is deleted
                .IsRequired(false); // Allow null in case user is deleted or system action
        }
    }
}
