using Microsoft.Extensions.Configuration;

namespace Infrastructure.Dapper
{
    public static class GetConnectionStringExtention
    {
        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private static string GetConnectionStringSafely(IConfiguration configuration, string name)
        {
            var connectionString = configuration.GetConnectionString(name);
            return connectionString ?? throw new InvalidOperationException($"Connection string '{name}' is not found or is null.");
        }

        public static string GetDefaultConnectionString()
        {
            var configuration = BuildConfiguration();
            return GetConnectionStringSafely(configuration, "DefaultConnection");
        }

    }
}
