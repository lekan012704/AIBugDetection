using Asp.Versioning.ApiExplorer;

namespace Web.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger();
        //app.UseSwaggerUI();
        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription description in app.DescribeApiVersions())
            {
                string url = $"/swagger/{description.GroupName}/swagger.json";
                string name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });

        return app;
    }
}
