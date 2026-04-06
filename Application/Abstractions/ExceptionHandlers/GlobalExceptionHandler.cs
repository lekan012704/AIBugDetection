using ErrorOr;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security;

namespace Application.Abstractions.ExceptionHandlers
{
    internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Exception occured: {Message}", exception.Message);

            var error = MapExceptionToError(exception);
            var statusCode = CustomResults.MapErrorTypeToStatusCode(error.Type);
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Type = CustomResults.GetErrorType(statusCode),
                Title = statusCode == StatusCodes.Status500InternalServerError
                    ? "Internal Server Error"
                    : "A handled exception occurred",
                Detail = error.Code,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        private static Error MapExceptionToError(Exception exception)
        {
            return exception switch
            {
                // Validation Errors
                ArgumentException => Error.Validation(
                    code: "InvalidArgument",
                    description: exception.Message),

                // Authorization Errors
                UnauthorizedAccessException => Error.Unauthorized(
                    code: "Unauthorized",
                    description: "You are not authorized to perform this action"),
                SecurityException => Error.Forbidden(
                    code: "SecurityViolation",
                    description: "Access to the resource is forbidden"),

                // Resource/Conflict Errors
                InvalidOperationException => Error.Conflict(
                    code: "InvalidOperation",
                    description: "The current state prevents this operation"),

                // Not Found Errors
                FileNotFoundException ex => Error.NotFound(
                    code: "FileNotFound",
                    description: $"File not found: {ex.FileName}"),
                DirectoryNotFoundException => Error.NotFound(
                    code: "DirectoryNotFound",
                    description: "The specified directory could not be found"),

                // Forbidden Errors
                System.Security.Authentication.AuthenticationException => Error.Forbidden(
                    code: "AuthenticationFailed",
                    description: "Authentication failed"),

                // Timeout and System Errors
                TimeoutException => Error.Failure(
                    code: "Timeout",
                    description: "The operation timed out"),
                OutOfMemoryException => Error.Failure(
                    code: "OutOfMemory",
                    description: "Insufficient memory to complete the operation"),
                StackOverflowException => Error.Failure(
                    code: "StackOverflow",
                    description: "Stack overflow occurred"),

                // Specific Application Exceptions
                ApplicationException => Error.Conflict(
                    code: "ApplicationError",
                    description: exception.Message),

                // Catch-all for unhandled exceptions
                _ => Error.Failure(
                    code: "UnhandledException",
                    description: $"An unexpected error occurred: {exception.GetType().Name}")
            };
        }
    }
}
