using Application.Abstractions.ExceptionHandlers;
using Application.Helper;
using ErrorOr;
using Infrastructure.Authentication.Custom.ApiKey;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthHeaderValidationService _headerValidation;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            IAuthHeaderValidationService headerValidation,
            ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _headerValidation = headerValidation;
            _logger = logger;
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    var validationResult = _headerValidation.ValidateAuthHeader(context.Request.Headers);

        //    if (validationResult.IsError)
        //    {
        //        _logger.LogWarning("Authentication failed: {Error}", validationResult.FirstError.Description);
        //        await HandleErrorResponse(context, validationResult.Errors);
        //        return;
        //    }

        //    var (merchantCode, apiSecretKey) = validationResult.Value;
        //    context.Items["MerchantCode"] = merchantCode;
        //    context.Items["ApiSecretKey"] = apiSecretKey;

        //    //Dev Remark: Maybe we put this in database but consider performance, use cache
        //    var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
        //    var appSettingsSection = appSettings.GetSection("AppSettings");
        //    var apiKey = appSettingsSection.Get<AppSettings>();

        //    if (!apiKey!.ApiAuthSecretKey!.Equals(apiSecretKey))
        //    {
        //        _logger.LogWarning("Authentication failed: {Error}", validationResult.FirstError.Description);
        //        await HandleErrorResponse(context, validationResult.Errors);
        //        return;
        //    }

        //    await _next(context);
        //}

        private static async Task HandleErrorResponse(HttpContext context, List<Error> errors)
        {
            var firstError = errors.First();

            var statusCode = CustomResults.MapErrorTypeToStatusCode(firstError.Type);

            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = firstError.Description,
                Type = CustomResults.GetErrorType(statusCode),
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
