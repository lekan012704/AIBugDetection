using Domain.Application.Entities.BugDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace Domain.Entites.Configurations;

public sealed class CodeAnalysisSessionConfiguration
    : IEntityTypeConfiguration<CodeAnalysisSession>
{
    public void Configure(EntityTypeBuilder<CodeAnalysisSession> builder)
    {
        builder.ToTable("CodeAnalysisSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.SubmissionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.FileName)
            .HasMaxLength(500);

        builder.Property(s => s.GitHubUrl)
            .HasMaxLength(1000);

        builder.Property(s => s.RawCode)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.DetectedLanguage)
            .HasMaxLength(100);

        builder.Property(s => s.DetectedFramework)
            .HasMaxLength(200);

        builder.Property(s => s.DetectedArchitecture)
            .HasMaxLength(200);

        // JSON columns
        builder.Property(s => s.DetectedPatterns)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.DetectedPrinciples)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.InjectedServices)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.CluesFound)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.AiProvider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.InitialObservations)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.FollowUpQuestions)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.UserAnswers)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.FinalAnalysis)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.PatternCompliance)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.ExecutiveSummary)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.ArchitectureAssessment)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.OverallCodeQuality)
            .HasMaxLength(50);

        builder.Property(s => s.RefactoringPriorities)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.QuickWins)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.LongTermRecommendations)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasPrincipalKey(u => u.KeycloakId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Issues)
            .WithOne(i => i.Session)
            .HasForeignKey(i => i.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Conversations)
            .WithOne(c => c.Session)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);
    }
}