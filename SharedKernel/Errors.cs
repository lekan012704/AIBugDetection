using ErrorOr;

namespace SharedKernel;

public static class Errors
{
    public static class Common
    {
        public static Error NotFound(string entityName, string identifier)
            => Error.NotFound(
                code: $"{entityName}.NotFound",
                description: $"{entityName} with identifier {identifier} was not found.");

        public static Error Validation(string entityName, string reason)
            => Error.Validation(
                code: $"{entityName}.InvalidInput",
                description: $"Invalid input for {entityName}: {reason}");

        public static Error Conflict(string entityName, string reason)
            => Error.Conflict(
                code: $"{entityName}.Conflict",
                description: $"Conflict in {entityName}: {reason}");

        public static Error Incomplete(string entityName, string reason)
            => Error.Conflict(
                code: $"{entityName}.Incomplete",
                description: $"Incomplete {entityName}: {reason}");

        public static Error Unauthorized(string entityName, string reason)
            => Error.Unauthorized(
                code: $"{entityName}.Unauthorized",
                description: $"You are not authorized to perform this action. {entityName}: {reason}");

        public static Error InvalidCredentials()
        => Error.Unauthorized(
            code: "Authentication.InvalidCredentials",
            description: "Invalid email or password");

        public static Error AccountLocked()
            => Error.Unauthorized(
                code: "Authentication.AccountLocked",
                description: "Account is locked or disabled.");

        public static Error AccountNotActivated()
            => Error.Unauthorized(
                code: "Authentication.AccountNotActivated",
                description: "Account is not activated.");
    }

    public static class Domain
    {
        public static Error RuleViolation(string ruleName, string description)
            => Error.Validation(
                code: $"Domain.RuleViolation.{ruleName}",
                description: description);
    }

    public static class Infrastructure
    {
        public static Error ExternalServiceFailure(string serviceName, string reason)
            => Error.Failure(
                code: $"{serviceName}.ExternalServiceError",
                description: $"External service {serviceName} failed: {reason}");

        public static Error DatabaseError(string operation, string details)
            => Error.Failure(
                code: "Database.OperationFailed",
                description: $"Database {operation} failed: {details}");

        public static Error DatabaseExceptionError(Exception operation, string details)
            => Error.Failure(
                code: "Database.OperationFailed",
                description: $"Database {operation.Message} failed: {details}");
    }
}

public readonly record struct ErrorOrResponse<T>
{
    public T? Data { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    private ErrorOrResponse(T? data, Error? error)
    {
        Data = data;
        Error = error;
    }

    public static ErrorOrResponse<T> Success(T data) => new(data, null);
    public static ErrorOrResponse<T> Failure(Error error) => new(default, error);
}

//public record Error(string Code, string Message);