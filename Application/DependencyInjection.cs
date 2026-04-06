using Application.Abstractions.Behaviors;
using Application.Abstractions.ExceptionHandlers;
using FluentValidation;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration _config)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            //Hangfire Configure Starts
            //services.AddHangfire(x => x.UseRecommendedSerializerSettings().UseSqlServerStorage(_config.GetConnectionString("HangFireConnection")));
            //services.AddHangfireServer();
            //Hangfire Configure Ends

            return services;
        }
    }
}
