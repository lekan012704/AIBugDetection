//using Microsoft.Extensions.DependencyInjection;
//using Polly;
//using Polly.Extensions.Http;
//using SharedKernel.Helpers.GenericHttpClientService;

//namespace SharedKernel
//{
//    /// <summary>
//    /// Adds the GenericHttpClientHandlerService and required HttpClient configurations to the service collection.
//    /// </summary>
//    public static class DependencyInjection
//    {
//        public static IServiceCollection AddSharedKernel(
//        this IServiceCollection services) =>
//        services
//            .AddGenericHttpClientService();

//        /// <param name="services">The service collection to add the services to.</param>
//        /// <returns>The service collection for chaining.</returns>
//        private static IServiceCollection AddGenericHttpClientService(
//            this IServiceCollection services)
//        {
//            services.AddHttpClient<IGenericHttpClientHandlerService, GenericHttpClientHandlerService>(client =>
//            {
//                client.Timeout = TimeSpan.FromMinutes(2);
//                client.DefaultRequestHeaders.Accept.Clear();
//            });

//            services.AddHttpClient();
//            return services;
//        }

//        /// <summary>
//        /// Creates a retry policy for transient HTTP errors.
//        /// </summary>
//        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
//        {
//            return HttpPolicyExtensions
//                .HandleTransientHttpError()
//                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
//                .WaitAndRetryAsync(
//                    3, // Number of retries
//                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
//                    onRetry: (outcome, timespan, retryAttempt, context) =>
//                    {
//                        // You could add logging here
//                        //var logger = context.GetLogger<YourClass>();
//                        // logger.LogWarning("Retrying HTTP request attempt {RetryAttempt} after {TimeBetweenRetries}s", retryAttempt, timespan.TotalSeconds);
//                    });
//        }

//        /// <summary>
//        /// Creates a circuit breaker policy to prevent repeated calls to failing services.
//        /// </summary>
//        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
//        {
//            return HttpPolicyExtensions
//                .HandleTransientHttpError()
//                .CircuitBreakerAsync(
//                    handledEventsAllowedBeforeBreaking: 5,
//                    durationOfBreak: TimeSpan.FromSeconds(30));
//        }
//    }
//}
