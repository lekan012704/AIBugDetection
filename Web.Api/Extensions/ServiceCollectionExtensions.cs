using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BUG DETECTION API Documentation.",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "Lekan Development Team",
                    Email = "durosinmiabdulsobuur@gmail.com",
                    Url = new Uri("http://icmaservices.com/"),
                }
            });

            o.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            o.MapType<List<IFormFile>>(() => new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                }
            });

            // ✅ Fix IFormFileCollection Swagger error
            o.MapType<IFormFileCollection>(() => new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                }
            });

            o.OperationFilter<FileUploadOperationFilter>();

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };

            o.AddSecurityDefinition(
                JwtBearerDefaults.AuthenticationScheme,
                securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new List<string>()
                }
            };

            o.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }
}

internal sealed class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        // Check if this operation has any IFormFile parameters
        var hasFormFile = context.ApiDescription
            .ParameterDescriptions
            .Any(p =>
                p.Type == typeof(IFormFile) ||
                p.Type == typeof(IFormFileCollection) ||
                p.Type == typeof(List<IFormFile>));

        if (!hasFormFile)
            return;

        operation.Parameters.Clear();

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties =
                            new Dictionary<string, OpenApiSchema>
                            {
                                ["submissionType"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description =
                                        "Snippet | File | GitHubUrl | MultipleFiles",
                                    Enum = new List<IOpenApiAny>
                                    {
                                        new OpenApiString("Snippet"),
                                        new OpenApiString("File"),
                                        new OpenApiString("GitHubUrl"),
                                        new OpenApiString("MultipleFiles")
                                    }
                                },
                                ["codeSnippet"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description =
                                        "Paste code here when using Snippet type",
                                    Nullable = true
                                },
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description =
                                        "Upload single file when using File type",
                                    Nullable = true
                                },
                                ["files"] = new OpenApiSchema
                                {
                                    Type = "array",
                                    Items = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    },
                                    Description =
                                        "Upload multiple files when using MultipleFiles type",
                                    Nullable = true
                                },
                                ["gitHubUrl"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description =
                                        "GitHub file URL when using GitHubUrl type. " +
                                        "e.g: https://github.com/user/repo/blob/main/file.cs",
                                    Nullable = true
                                },
                                ["language"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description =
                                        "Optional language hint e.g: csharp, python, javascript",
                                    Nullable = true
                                }
                            },
                        Required = new HashSet<string> { "submissionType" }
                    }
                }
            }
        };
    }
}