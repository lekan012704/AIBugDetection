//using Asp.Versioning.ApiExplorer;
//using Microsoft.Extensions.Options;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace Web.Api.OpenApi
//{
//    internal sealed class ConfigureOpenApiOptions : IConfigureNamedOptions<SwaggerGenOptions>
//    {
//        private readonly IApiVersionDescriptionProvider _provider;
//        private readonly IConfiguration _configuration;

//        public ConfigureOpenApiOptions(
//            IApiVersionDescriptionProvider provider,
//            IConfiguration configuration)
//        {
//            _provider = provider;
//            _configuration = configuration;
//        }

//        public void Configure(SwaggerGenOptions options)
//        {
//            foreach (var description in _provider.ApiVersionDescriptions)
//            {
//                options.SwaggerDoc(
//                    description.GroupName,
//                    CreateVersionInfo(description));
//            }

//            // Configure common Swagger options here
//            options.RouteTemplate = "openapi/{documentName}.json";

//            // Add other Swagger configurations as needed
//            options.OperationFilter<AddApiVersionParameterFilter>();
//            options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
//            options.EnableAnnotations();
//        }

//        public void Configure(string? name, SwaggerGenOptions options) => Configure(options);

//        private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
//        {
//            var info = new OpenApiInfo()
//            {
//                Title = $"SelfService API {description.GroupName.ToUpperInvariant()}",
//                Version = description.ApiVersion.ToString(),
//                Description = description.IsDeprecated
//                    ? "This API version has been deprecated."
//                    : string.Empty
//            };

//            return info;
//        }
//    }

//    public class AddApiVersionParameterFilter : IOperationFilter
//    {
//        public void Apply(OpenApiOperation operation, OperationFilterContext context)
//        {
//            var apiVersion = context.GetApiVersion();
//            if (apiVersion == null) return;

//            operation.Parameters ??= [];
//            operation.Parameters.Add(new OpenApiParameter
//            {
//                Name = "api-version",
//                In = ParameterLocation.Query,
//                Required = true,
//                Schema = new OpenApiSchema { Type = "string" }
//            });
//        }
//    }

//    public class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
//    {
//        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
//        {
//            var replacements = new OpenApiPaths();
//            foreach (var (key, value) in swaggerDoc.Paths)
//            {
//                replacements.Add(key.Replace("{version}", swaggerDoc.Info.Version), value);
//            }
//            swaggerDoc.Paths = replacements;
//        }
//    }
//}
