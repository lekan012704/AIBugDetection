using Application;
using Asp.Versioning.ApiExplorer;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Scalar.AspNetCore;
using Serilog;
using SharedKernel;
using System.Reflection;
using Web.Api;
using Web.Api.Extensions;

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("Logs/applog-.txt", rollingInterval: RollingInterval.Day)
        .CreateBootstrapLogger();

    Log.Information("BUG DETECTION Api Starting..");


    var builder = WebApplication.CreateBuilder(args);
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = null;
    });

    // ✅ This tells ASP.NET Core to handle multipart form data
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 104857600; // 100MB max
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartHeadersLengthLimit = int.MaxValue;
    });

    builder.Host.UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration));

    // ✅ Register services
    builder.Services.AddSwaggerGenWithAuth();
    builder.Services
        .AddApplication(builder.Configuration)
        .AddPresentation()
        .AddInfrastructure(builder.Configuration)  // ← AddHealthChecks is inside here
        .AddSharedKernel();

    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
    
    builder.Services.AddHealthChecksUI(options =>
    {
        options.AddHealthCheckEndpoint("main", "/health");
    }).AddInMemoryStorage();
    // ✅ Add before builder.Build()
  

    // Load env vars early
    DotNetEnv.Env.Load();

    var app = builder.Build();

    // ✅ Configure HTTP pipeline in correct order

    if (app.Environment.IsDevelopment() ||
        app.Environment.EnvironmentName.Equals("test",
            StringComparison.InvariantCultureIgnoreCase))
    {
        app.UseCors(corsBuilder =>
        {
            corsBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        app.UseSwaggerUI(options =>
        {
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint(
                    $"/openapi/{description.GroupName}.json",
                    $"BUG DETECTION API {description.GroupName.ToUpperInvariant()}");
            }
        });

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        foreach (ApiVersionDescription description in app.DescribeApiVersions())
        {
            app.MapScalarApiReference(opt =>
            {
                opt.Title = $"BUG DETECTION API {description.GroupName.ToUpperInvariant()}";    
                opt.Theme = ScalarTheme.Mars;
                opt.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
            });
        }
    }
    else
    {
        app.UseSwaggerUI(options =>
        {
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint(
                    $"/openapi/{description.GroupName}.json",
                    $"BUG DETECTION API {description.GroupName.ToUpperInvariant()}");
            }
        });

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });
    }

    // ✅ Logging middleware first
    app.UseRequestContextLogging();
    app.UseSerilogRequestLogging();

    // ✅ Exception handler before auth
    app.UseExceptionHandler();

    // ✅ Auth middleware — must be in this exact order
    app.UseAuthentication();
    app.UseAuthorization();

    // ✅ Map endpoints AFTER auth middleware
    app.MapEndpoints();

    // ✅ Map controllers — UNCOMMENTED
    app.MapControllers();

    // ✅ Health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/health-ui";
    });

    //app.AddHangfireBackgroundJobs(builder.Configuration);

    Log.Information("BUG DETECTION Api Starting..");
    await app.RunAsync();
    Log.Information("BUG DETECTION Api Started..");
}
catch (Exception exception)
{
    Log.Fatal(exception, "Host terminated unexpectedly");
}
finally
{
    Log.Information("BUG DETECTION Api Shutting down");
    Log.CloseAndFlush();
}

namespace Web.Api
{
    public partial class Program;
}