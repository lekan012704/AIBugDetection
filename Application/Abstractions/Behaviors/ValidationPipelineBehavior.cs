using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {   
        if (!_validators.Any()) 
        {
            return await next();
        }

        var validationFailures = await ValidateRequestAsync(request, cancellationToken);

        if (validationFailures.Length == 0)
        {
            return await next();
        }       

        return await HandleValidationFailuresAsync(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateRequestAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator =>
                validator.ValidateAsync(context, cancellationToken)));

        return [.. validationResults
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)];
    }

    private Task<TResponse> HandleValidationFailuresAsync(ValidationFailure[] failures)
    {
        var validationErrors = CreateValidationErrors(failures);

        if (IsGenericErrorOr(typeof(TResponse)))
        {
            return Task.FromResult(CreateGenericErrorResponse(validationErrors));
        }

        if (typeof(TResponse) == typeof(IErrorOr))
        {
            return Task.FromResult((TResponse)(object)validationErrors);
        }

        throw new ValidationException(failures);
    }

    private static bool IsGenericErrorOr(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IErrorOr<>);

    private static TResponse CreateGenericErrorResponse(ValidationExceptions validationErrors)
    {
        var resultType = typeof(TResponse).GetGenericArguments()[0];
        var errorOrType = typeof(IErrorOr<>).MakeGenericType(resultType);

        var valueMethod = errorOrType.GetMethod(nameof(IErrorOr<object>.Value))
            ?? throw new InvalidOperationException($"Could not find Value method on {errorOrType.Name}");

        return (TResponse)valueMethod.Invoke(null, [validationErrors])!;
    }

    private static ValidationExceptions CreateValidationErrors(ValidationFailure[] failures) =>
        ValidationExceptions.Create(
            [.. failures.Select(failure =>
                Error.Failure(failure.ErrorCode, failure.ErrorMessage))]);
}
