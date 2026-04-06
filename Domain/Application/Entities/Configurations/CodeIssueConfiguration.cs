using Domain.Application.Entities.BugDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Domain.Entites.Configurations;

public sealed class CodeIssueConfiguration
    : IEntityTypeConfiguration<CodeIssue>
{
    public void Configure(EntityTypeBuilder<CodeIssue> builder)
    {
        builder.ToTable("CodeIssues");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.IssueType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Description)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Severity)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.CodeSnippet)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.PatternViolated)
            .HasMaxLength(200);

        builder.Property(i => i.PrincipleViolated)
            .HasMaxLength(100);

        builder.Property(i => i.ArchitectureLayerViolated)
            .HasMaxLength(200);

        builder.Property(i => i.DeveloperIntent)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Contradiction)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.SuggestedFix)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.FixedCode)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Explanation)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.RefactoringSteps)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.FoundInPhase)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.SessionId);
        builder.HasIndex(i => i.Severity);
        builder.HasIndex(i => i.IssueType);
    }
}