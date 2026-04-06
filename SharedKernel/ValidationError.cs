using ErrorOr;

namespace SharedKernel;

public sealed class ValidationExceptions : IErrorOr
{
    public ValidationExceptions(List<Error> errors)
    {
        Errors = errors ?? [];
    }

    public List<Error> Errors { get; }

    public bool IsError => Errors.Any();

    public static ValidationExceptions Create(List<Error> errors)
        => new(errors);

    public static ValidationExceptions Create(Error error)
        => new([error]);

    public static ValidationExceptions Create(string code, string description)
        => Create(Error.Failure(code, description));
}
