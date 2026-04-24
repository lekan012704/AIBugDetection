using Application.Abstractions.AI;
using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.Authentication.KeyCloak;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.BugDetection;
using Application.Abstractions.EntityRepositories.Settings;
using Application.Abstractions.EntityRepositories.Users;
using Application.Abstractions.GenericRepository;
using Application.Caching;
using Application.Dapper;
using Application.Helper;
using Application.Models;
using Application.Scheduler;
using Asp.Versioning;
using Dapper;
using Domain.Application.Entities.Outbox;
using EmailSenderUtility.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Humanizer;
using Infrastructure.AI;
using Infrastructure.Authentication.Custom.Jwt;
using Infrastructure.Authentication.KeyCloak;
using Infrastructure.Authorization;
using Infrastructure.Caching;
using Infrastructure.Dapper;
using Infrastructure.Database;
using Infrastructure.EntityRepositories.BugDetection;
using Infrastructure.EntityRepositories.Outbox;
using Infrastructure.EntityRepositories.Users;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using SharedKernel;
using SharedKernel.Helpers.GenericHttpClientService;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddWkHtmlToPdfService(configuration)
            .AddCaching(configuration)
            .AddApiVersioning()
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();    
        //    .AddEmailingService(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped(
            typeof(IRepositoryAsync<,>),
            typeof(GenericRepository.RepositoryAsync<,>));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUserRespository, UserRepository>();
        services.AddSingleton<ICacheService, CacheService>();
        //services.AddScoped<IContextSeed, ContextSeed>();
        services.AddScoped<IUserContext, UserContext>();
        //services.AddScoped<IHandleException, HandleException>();
        //services.AddScoped<IEmailAddressService, EmailAddressService>();
        services.AddScoped<IEntityManagerService, EntityManagerService>();
        services.AddScoped<IGenericHttpClientHandlerService, GenericHttpClientHandlerService>();
        services.AddScoped<IGitHubService, GitHubService>();
        services.AddScoped<IClaudeService, ClaudeService>();
        services.AddScoped<IOpenAiService, OpenAiService>();
        services.AddScoped<IAiBugDetectionService, AiBugDetectionService>();
        services.AddScoped<ICodeIssueRepository, CodeIssueRepository>();
        services.AddScoped<IAnalysisConversationRepository, AnalysisConversationRepository>();
        services.AddScoped<IBugItemRepository, BugItemRepository>();
        services.AddScoped<IBugReportRepository, BugReportRepository>();
        services.AddScoped<ICodeAnalysisSessionRepository, CodeAnalysisSessionRepository>();
        

        // ✅ Removed — Keycloak issues tokens, we don't create our own
        // services.AddSingleton<IPasswordHasher, PasswordHasher>();
        // services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }

    private static IServiceCollection AddDatabase(
     this IServiceCollection services,
     IConfiguration configuration)
    {
        // ✅ Read from environment variable first
        // Fly.io sets DATABASE_URL automatically
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(
                "Database connection string not configured");

        // ✅ Convert Fly.io postgres:// URL to Npgsql format
        if (connectionString.StartsWith("postgres://") ||
            connectionString.StartsWith("postgresql://"))
        {
            connectionString = ConvertToNpgsqlConnectionString(
                connectionString);
        }

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            throw new Exception("DATABASE_URL is not set");
        }

        var connectionStrings = ConvertToNpgsqlConnectionString(databaseUrl);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            });
        });
        services.AddScoped<IUnitOfWork>(sp =>
         sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<AppSettings>(options =>
        {
            options.ApplicationBaseUrl =
                Environment.GetEnvironmentVariable("APP_BASE_URL") ??
                configuration["AppSettings:ApplicationBaseUrl"] ?? "";

            options.UseMockAiResponse =
                bool.Parse(
                    Environment.GetEnvironmentVariable("USE_MOCK_AI") ??
                    configuration["AppSettings:UseMockAiResponse"] ??
                    "true");

            // ── Gemini ──────────────────────────────────
            options.GeminiApiKey =
                Environment.GetEnvironmentVariable("GEMINI_API_KEY") ??
                configuration["AppSettings:GeminiApiKey"] ?? "";    

            options.GeminiApiUrl =
                Environment.GetEnvironmentVariable("GEMINI_API_URL") ??
                configuration["AppSettings:GeminiApiUrl"] ?? "";

            options.GeminiModel =
                Environment.GetEnvironmentVariable("GEMINI_MODEL") ??
                configuration["AppSettings:GeminiModel"] ?? "";

            // ── Claude ──────────────────────────────────
            options.ClaudeOpenApiKey =
                Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ??
                configuration["AppSettings:ClaudeOpenApiKey"] ?? "";

            options.ClaudeOpenApiUrl =
                Environment.GetEnvironmentVariable("CLAUDE_API_URL") ??
                configuration["AppSettings:ClaudeOpenApiUrl"] ?? "";

            options.ClaudeModel =
                Environment.GetEnvironmentVariable("CLAUDE_MODEL") ??
                configuration["AppSettings:ClaudeModel"] ?? "";

            options.AnthropicVersion =
                Environment.GetEnvironmentVariable("ANTHROPIC_VERSION") ??
                configuration["AppSettings:AnthropicVersion"] ?? "";

            // ── OpenAI ──────────────────────────────────
            options.OpenAiApiKey =
                Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                configuration["AppSettings:OpenAiApiKey"] ?? "";

            options.OpenAiApiUrl =
                Environment.GetEnvironmentVariable("OPENAI_API_URL") ??
                configuration["AppSettings:OpenAiApiUrl"] ?? "";

            options.OpenAiModel =
                Environment.GetEnvironmentVariable("OPENAI_MODEL") ??
                configuration["AppSettings:OpenAiModel"] ?? "";
        });

        // ── Keycloak ────────────────────────────────────
        services.Configure<KeycloakOptions>(options =>
        {
            options.BaseUrl =
                Environment.GetEnvironmentVariable("KEYCLOAK_BASEURL") ??
                configuration["KeyCloak:BaseUrl"] ?? "";

            options.Authority =
                Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY") ??
                configuration["KeyCloak:Authority"] ?? "";

            options.ClientId =
                Environment.GetEnvironmentVariable("KEYCLOAK_CLIENTID") ??
                configuration["KeyCloak:ClientId"] ?? "";

            options.TokenUrl =
                Environment.GetEnvironmentVariable("KEYCLOAK_TOKENURL") ??
                configuration["KeyCloak:TokenUrl"] ?? "";

            options.AdminUrl =
                Environment.GetEnvironmentVariable("KEYCLOAK_ADMINURL") ??
                configuration["KeyCloak:AdminUrl"] ?? "";

            options.AuthClientId =
                Environment.GetEnvironmentVariable("KEYCLOAK_AUTHCLIENTID") ??
                configuration["KeyCloak:AuthClientId"] ?? "";

            options.AuthClientSecret =
                Environment.GetEnvironmentVariable("KEYCLOAK_AUTHCLIENTSECRET") ??
                configuration["KeyCloak:AuthClientSecret"] ?? "";

            options.AdminClientId =
                Environment.GetEnvironmentVariable("KEYCLOAK_ADMINCLIENTID") ??
                configuration["KeyCloak:AdminClientId"] ?? "";

            options.AdminClientSecret =
                Environment.GetEnvironmentVariable("KEYCLOAK_ADMINCLIENTSECRET") ??
                configuration["KeyCloak:AdminClientSecret"] ?? "";
        });

        // ── SMTP ────────────────────────────────────────
        services.Configure<SmtpOptions>(options =>
        {
            options.Host =
                Environment.GetEnvironmentVariable("SMTP_HOST") ??
                configuration["SmtpOptions:Host"] ?? "";

            options.Port = int.Parse(
                Environment.GetEnvironmentVariable("SMTP_PORT") ??
                configuration["SmtpOptions:Port"] ?? "587");

            options.Username =
                Environment.GetEnvironmentVariable("SMTP_USERNAME") ??
                configuration["SmtpOptions:Username"] ?? "";

            options.Password =
                Environment.GetEnvironmentVariable("SMTP_PASSWORD") ??
                configuration["SmtpOptions:Password"] ?? "";

            options.DisplayName =
                Environment.GetEnvironmentVariable("SMTP_DISPLAY_NAME") ??
                configuration["SmtpOptions:DisplayName"] ?? "";

            options.TestEmail =
                Environment.GetEnvironmentVariable("SMTP_TEST_EMAIL") ??
                configuration["SmtpOptions:TestEmail"] ?? "";

            options.UseSsl = bool.Parse(
                Environment.GetEnvironmentVariable("SMTP_USE_SSL") ??
                configuration["SmtpOptions:UseSsl"] ?? "true");

            options.LicenseKey =
                Environment.GetEnvironmentVariable("SMTP_LICENSE_KEY") ??
                configuration["SmtpOptions:LicenseKey"] ?? "";
        });// In DependencyInjection.cs — AddDatabase method
           // Map environment variables to AppSettings
        services.Configure<AppSettings>(options =>
        {
            options.ApplicationBaseUrl =
                Environment.GetEnvironmentVariable("APP_BASE_URL") ??
                configuration["AppSettings:ApplicationBaseUrl"] ?? "";

            options.UseMockAiResponse =
                bool.Parse(
                    Environment.GetEnvironmentVariable("USE_MOCK_AI") ??
                    configuration["AppSettings:UseMockAiResponse"] ??
                    "true");

            // ── Gemini ──────────────────────────────────
            options.GeminiApiKey =
                Environment.GetEnvironmentVariable("GEMINI_API_KEY") ??
                configuration["AppSettings:GeminiApiKey"] ?? "";

            options.GeminiApiUrl =
                Environment.GetEnvironmentVariable("GEMINI_API_URL") ??
                configuration["AppSettings:GeminiApiUrl"] ?? "";

            options.GeminiModel =
                Environment.GetEnvironmentVariable("GEMINI_MODEL") ??
                configuration["AppSettings:GeminiModel"] ?? "";

            // ── Claude ──────────────────────────────────
            options.ClaudeOpenApiKey =
                Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ??
                configuration["AppSettings:ClaudeOpenApiKey"] ?? "";

            options.ClaudeOpenApiUrl =
                Environment.GetEnvironmentVariable("CLAUDE_API_URL") ??
                configuration["AppSettings:ClaudeOpenApiUrl"] ?? "";

            options.ClaudeModel =
                Environment.GetEnvironmentVariable("CLAUDE_MODEL") ??
                configuration["AppSettings:ClaudeModel"] ?? "";

            options.AnthropicVersion =
                Environment.GetEnvironmentVariable("ANTHROPIC_VERSION") ??
                configuration["AppSettings:AnthropicVersion"] ?? "";

            // ── OpenAI ──────────────────────────────────
            options.OpenAiApiKey =
                Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                configuration["AppSettings:OpenAiApiKey"] ?? "";

            options.OpenAiApiUrl =
                Environment.GetEnvironmentVariable("OPENAI_API_URL") ??
                configuration["AppSettings:OpenAiApiUrl"] ?? "";

            options.OpenAiModel =
                Environment.GetEnvironmentVariable("OPENAI_MODEL") ??
                configuration["AppSettings:OpenAiModel"] ?? "";
        });

        // ── SMTP ────────────────────────────────────────
        services.Configure<SmtpOptions>(options =>
        {
            options.Host =
                Environment.GetEnvironmentVariable("SMTP_HOST") ??
                configuration["SmtpOptions:Host"] ?? "";

            options.Port = int.Parse(
                Environment.GetEnvironmentVariable("SMTP_PORT") ??
                configuration["SmtpOptions:Port"] ?? "587");

            options.Username =
                Environment.GetEnvironmentVariable("SMTP_USERNAME") ??
                configuration["SmtpOptions:Username"] ?? "";

            options.Password =
                Environment.GetEnvironmentVariable("SMTP_PASSWORD") ??
                configuration["SmtpOptions:Password"] ?? "";

            options.DisplayName =
                Environment.GetEnvironmentVariable("SMTP_DISPLAY_NAME") ??
                configuration["SmtpOptions:DisplayName"] ?? "";

            options.TestEmail =
                Environment.GetEnvironmentVariable("SMTP_TEST_EMAIL") ??
                configuration["SmtpOptions:TestEmail"] ?? "";

            options.UseSsl = bool.Parse(
                Environment.GetEnvironmentVariable("SMTP_USE_SSL") ??
                configuration["SmtpOptions:UseSsl"] ?? "true");

            options.LicenseKey =
                Environment.GetEnvironmentVariable("SMTP_LICENSE_KEY") ??
                configuration["SmtpOptions:LicenseKey"] ?? "";
        });

        return services;
    }

    private static string ConvertToNpgsqlConnectionString(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url), "DATABASE_URL is empty");

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};" +
               $"Port={uri.Port};" +
               $"Database={database};" +
               $"Username={userInfo[0]};" +
               $"Password={Uri.UnescapeDataString(userInfo[1])};" +
               $"SSL Mode=true;"; // ✅ FIXED
    }

    private static IServiceCollection AddHealthChecks(
     this IServiceCollection services,
     IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (string.IsNullOrWhiteSpace(databaseUrl))
            throw new Exception("DATABASE_URL is not set");

        var connectionString = ConvertToNpgsqlConnectionString(databaseUrl);

        var healthChecks = services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: connectionString,
                name: "AppDatabase",
                timeout: TimeSpan.FromSeconds(30),
                failureStatus: HealthStatus.Degraded,
                tags: ["db", "postgres"]);

        var keycloakUrl =
            Environment.GetEnvironmentVariable("KEYCLOAK_BASEURL")
            ?? configuration["KeyCloak:BaseUrl"];

        if (!string.IsNullOrWhiteSpace(keycloakUrl))
        {
            healthChecks.AddUrlGroup(
                uri: new Uri(keycloakUrl),
                httpMethod: HttpMethod.Get,
                name: "keycloak",
                failureStatus: HealthStatus.Degraded,
                tags: ["auth", "keycloak"]);
        }

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
     this IServiceCollection services,
     IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority =
    Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY")
    ?? configuration["KeyCloak:Authority"];
            options.RequireHttpsMetadata = false; // ⚠️ Set true in production

            // ✅ Remove options.Audience = configuration["KeyCloak:ClientId"]
            // Instead validate against all audiences in the token

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,

                // ✅ Validate against all audiences Keycloak puts in the token
                ValidateAudience = true,
                ValidAudiences = new[]
                {
                "realm-management",
                "account",
                "bug-detector-api"
                },

                NameClaimType = "preferred_username",
                RoleClaimType = "roles"
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError(
                        context.Exception,
                        "Keycloak authentication failed: {Message}",
                        context.Exception.Message);
                    return Task.CompletedTask;
                },

                // ✅ Add this — tells you exactly why token was rejected
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError(
                        "Token rejected. Error: {Error} Description: {Description}",
                        context.Error,
                        context.ErrorDescription);
                    return Task.CompletedTask;
                },

                // ✅ Add this — confirms token was validated successfully
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    var username = context.Principal?
                        .FindFirst("preferred_username")?.Value;
                    logger.LogInformation(
                        "Token validated for user: {Username}", username);
                    return Task.CompletedTask;
                }
            };
        });

        services.Configure<KeycloakOptions>(configuration.GetSection("KeyCloak"));

        var keycloakBaseUrl =
        Environment.GetEnvironmentVariable("KEYCLOAK_BASEURL")
        ?? configuration["KeyCloak:BaseUrl"];

        if (string.IsNullOrWhiteSpace(keycloakBaseUrl))
            throw new ArgumentNullException("KeyCloak:BaseUrl is not configured");

        services.AddHttpClient<IKeyCloakService, KeyCloakService>(client =>
        {
            client.BaseAddress = new Uri(keycloakBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpContextAccessor();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(
        this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddTransient<IPermissionRepository, PermissionRepository>();
        services.AddScoped<PermissionProvider>();
        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
    private static IServiceCollection AddWkHtmlToPdfService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Uncomment when WkHtmlToPdf is ready
        return services;
    }

    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Uncomment when Redis is ready
        // string connectionString = configuration.GetConnectionString("Cache") ??
        //     throw new ArgumentNullException(nameof(configuration));
        // services.AddStackExchangeRedisCache(options =>
        //     options.Configuration = connectionString);

#pragma warning disable EXTEXP0018
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        return services;
    }

    private static IServiceCollection AddApiVersioning(
        this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    private static IServiceCollection AddEmailingService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var smtpOptions = configuration.GetSection("SmtpOptions").Get<SmtpOptions>()
            ?? throw new InvalidOperationException("SMTP options are required.");

        var sendGridOptions = configuration.GetSection("SendGridOptions").Get<SendGridOptions>()
            ?? throw new InvalidOperationException("SendGrid options are required.");

        bool useSmtp = configuration["AppSettings:UseSmtp"] != null
            && Convert.ToBoolean(configuration["AppSettings:UseSmtp"]);

        services.AddEmailService(configurator =>
        {
            if (useSmtp)
                configurator.UseSmtp(smtpConfigurator =>
                {
                    smtpConfigurator
                        .Host(smtpOptions.Host)
                        .Port(smtpOptions.Port)
                        .Username(smtpOptions.Username)
                        .Password(smtpOptions.Password)
                        .Displayname(smtpOptions.DisplayName)
                        .TestEmail(smtpOptions.TestEmail)
                        .SetLicenseKey(smtpOptions.LicenseKey)
                        .EnableDebug(false);

                    if (smtpOptions.UseSsl)
                        smtpConfigurator.UseSsl();
                    if (smtpOptions.SetAsDefaultProvider)
                        smtpConfigurator.SetAsDefaultProvider();
                });
            else
                configurator.UseSendGrid(sendGridConfigurator =>
                {
                    sendGridConfigurator
                        .SetApiKey(sendGridOptions.ApiKey)
                        .SetTestEmail(sendGridOptions.TestEmail);
                });
        });

        return services;
    }

    private static IServiceCollection AddQuartzBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddQuartz();
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();

        return services;
    }

    //public static WebApplication AddHangfireBackgroundJobs(
    //    this WebApplication app,
    //    IConfiguration configuration)
    //{
    //    var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();

    //    app.UseHangfireDashboard("/Pusher", new DashboardOptions
    //    {
    //        DashboardTitle = "AIBugDetection Jobs",
    //        Authorization = [new HangfireAuthorizationFilter()],
    //        IsReadOnlyFunc = (DashboardContext context) => false,
    //        AppPath = $"{appSettings.ApplicationBaseUrl}",
    //    });

    //    HangFireService.InitialiseService();

    //    return app;
    //}

    private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string[] _roles;

        public HangfireAuthorizationFilter(params string[] roles)
        {
            _roles = roles;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
            // TODO: Add proper role-based check using Keycloak claims
            return true;
        }
    }
}