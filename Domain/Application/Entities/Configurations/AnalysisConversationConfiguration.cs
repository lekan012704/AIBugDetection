using Domain.Application.Entities.BugDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Domain.Entites.Configurations;

public sealed class AnalysisConversationConfiguration
    : IEntityTypeConfiguration<AnalysisConversation>
{
    public void Configure(EntityTypeBuilder<AnalysisConversation> builder)
    {
        builder.ToTable("AnalysisConversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Message)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Phase)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.SessionId);
    }
}