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
            .AddAuthorizationInternal()
            .AddEmailingService(configuration);

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
        string? connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(
                $"Database connection string is not configured for {nameof(AddDatabase)}");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName, Schemas.Default);
                sqlOptions.EnableRetryOnFailure(3);
                sqlOptions.CommandTimeout(30);
            });
        });


        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        //services.AddScoped<IDapperFactory>(sp =>
        //{
        //    var handleException = sp.GetRequiredService<IHandleException>();
        //    return new DapperFactory(configuration, handleException);
        //});

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));
        services.Configure<SendGridOptions>(configuration.GetSection("SendGridOptions"));

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        return services;
    }

    private static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(
                connectionString: configuration.GetConnectionString("Database")
                    ?? throw new Exception("Database connection string cannot be null"),
                healthQuery: "SELECT 1;",
                name: "AppDatabase",
                timeout: TimeSpan.FromSeconds(30),
                failureStatus: HealthStatus.Degraded,
                tags: ["db", "sql", "sqlserver"])
            // ✅ Uncomment once Keycloak is running
            .AddUrlGroup(
                uri: new Uri(configuration["KeyCloak:BaseUrl"]
                    ?? throw new Exception("Keycloak BaseUrl is not configured")),
                httpMethod: HttpMethod.Get,
                name: "keycloak",
                failureStatus: HealthStatus.Degraded,
                tags: ["auth", "keycloak"]);

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
            options.Authority = configuration["KeyCloak:Authority"];
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

        services.AddHttpClient<IKeyCloakService,KeyCloakService >(client =>
        {
            client.BaseAddress = new Uri(
                configuration["KeyCloak:BaseUrl"]
                ?? throw new ArgumentNullException("KeyCloak:BaseUrl is not configured"));
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