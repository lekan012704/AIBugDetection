using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.ExceptionHandlers
{
    public static class CustomResults
    {
        public static IResult Problem<T>(ErrorOr<T> errorOr)
        {
            if (!errorOr.IsError)
            {
                return Results.Problem();
                //throw new InvalidOperationException("Cannot create a ProblemDetails from a successful ErrorOr result");
            }

            return Results.Problem(
                title: GetTitle(errorOr.Errors[0]),
                detail: GetDetail(errorOr.Errors),
                type: GetErrorType(errorOr.Errors[0].Type),
                statusCode: MapErrorTypeToStatusCode(errorOr.Errors[0].Type),
                extensions: GetErrors(errorOr.Errors));
        }

        public static string GetTitle(Error error) =>
            error.Type switch
            {
                ErrorType.Validation => "Validation Error",
                ErrorType.NotFound => "Resource Not Found",
                ErrorType.Conflict => "Conflict Error",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                _ => "Server Error"
            };

        public static string GetDetail(IReadOnlyList<Error> errors) =>
                errors.Count == 1
                    ? errors[0].Description
                    : $"Multiple errors occurred: {string.Join(", ", errors.Select(e => e.Description))}";

        public static string GetErrorType(ErrorType errorType) =>
                errorType switch
                {
                    ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                    ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

        public static string GetErrorType(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest =>
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            StatusCodes.Status404NotFound =>
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            StatusCodes.Status409Conflict =>
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            StatusCodes.Status401Unauthorized =>
                "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            StatusCodes.Status403Forbidden =>
                "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };

        public static int MapErrorTypeToStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

        private static Dictionary<string, object?>? GetErrors(IReadOnlyList<Error> errors)
        {
            if (!errors.Any(e => e.Type == ErrorType.Validation))
            {
                return null;
            }

            var validationErrors = errors
                .Where(e => e.Type == ErrorType.Validation)
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray() as object);

            return new Dictionary<string, object?>
            {
                { "errors", validationErrors }
            };
        }

        // Overload for Success type with no value to return, just want to return success.
        public static IResult Problem(ErrorOr<Success> message)
        {
            return Problem<Success>(message);
        }
    }
}
