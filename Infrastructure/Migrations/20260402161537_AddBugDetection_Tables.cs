using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBugDetection_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_KeycloakId",
                schema: "BugDetection",
                table: "Users",
                column: "KeycloakId");

            migrationBuilder.CreateTable(
                name: "BugReports",
                schema: "BugDetection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubmissionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GitHubUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgrammingLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiProvider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FallbackUsed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalBugsFound = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BugReports_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "BugDetection",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeAnalysisSessions",
                schema: "BugDetection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmissionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GitHubUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RawCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectedLanguage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DetectedFramework = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DetectedArchitecture = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DetectedPatterns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedPrinciples = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InjectedServices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CluesFound = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AiProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FallbackUsed = table.Column<bool>(type: "bit", nullable: false),
                    InitialObservations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowUpQuestions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAnswers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatternCompliance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchitectureAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallCodeQuality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefactoringPriorities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuickWins = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LongTermRecommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalIssuesFound = table.Column<int>(type: "int", nullable: false),
                    CriticalIssues = table.Column<int>(type: "int", nullable: false),
                    HighIssues = table.Column<int>(type: "int", nullable: false),
                    MediumIssues = table.Column<int>(type: "int", nullable: false),
                    LowIssues = table.Column<int>(type: "int", nullable: false),
                    InitialAnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalAnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeAnalysisSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeAnalysisSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "BugDetection",
                        principalTable: "Users",
                        principalColumn: "KeycloakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BugItems",
                schema: "BugDetection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BugReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: true),
                    EndLineNumber = table.Column<int>(type: "int", nullable: true),
                    CodeSnippet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedFix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FixedCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BugItems_BugReports_BugReportId",
                        column: x => x.BugReportId,
                        principalSchema: "BugDetection",
                        principalTable: "BugReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisConversations",
                schema: "BugDetection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisConversations_CodeAnalysisSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "BugDetection",
                        principalTable: "CodeAnalysisSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeIssues",
                schema: "BugDetection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: true),
                    EndLineNumber = table.Column<int>(type: "int", nullable: true),
                    CodeSnippet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatternViolated = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrincipleViolated = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ArchitectureLayerViolated = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeveloperIntent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contradiction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedFix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FixedCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefactoringSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FoundInPhase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeIssues_CodeAnalysisSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "BugDetection",
                        principalTable: "CodeAnalysisSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisConversations_SessionId",
                schema: "BugDetection",
                table: "AnalysisConversations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BugItems_BugReportId",
                schema: "BugDetection",
                table: "BugItems",
                column: "BugReportId");

            migrationBuilder.CreateIndex(
                name: "IX_BugReports_UserId",
                schema: "BugDetection",
                table: "BugReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeAnalysisSessions_CreatedAt",
                schema: "BugDetection",
                table: "CodeAnalysisSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CodeAnalysisSessions_Status",
                schema: "BugDetection",
                table: "CodeAnalysisSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CodeAnalysisSessions_UserId",
                schema: "BugDetection",
                table: "CodeAnalysisSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeIssues_IssueType",
                schema: "BugDetection",
                table: "CodeIssues",
                column: "IssueType");

            migrationBuilder.CreateIndex(
                name: "IX_CodeIssues_SessionId",
                schema: "BugDetection",
                table: "CodeIssues",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeIssues_Severity",
                schema: "BugDetection",
                table: "CodeIssues",
                column: "Severity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisConversations",
                schema: "BugDetection");

            migrationBuilder.DropTable(
                name: "BugItems",
                schema: "BugDetection");

            migrationBuilder.DropTable(
                name: "CodeIssues",
                schema: "BugDetection");

            migrationBuilder.DropTable(
                name: "BugReports",
                schema: "BugDetection");

            migrationBuilder.DropTable(
                name: "CodeAnalysisSessions",
                schema: "BugDetection");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_KeycloakId",
                schema: "BugDetection",
                table: "Users");
        }
    }
}
