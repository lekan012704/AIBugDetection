using ErrorOr;

namespace Web.Api.Extensions;

public static class ErrorOrExtensions
{
    public static TOut MatchWithoutValue1<TOut>(
        this ErrorOr<Success> errorOr,
        Func<TOut> onSuccess,
        Func<IReadOnlyList<Error>, TOut> onError)
    {
        return errorOr.IsError ? onError(errorOr.Errors) : onSuccess();
    }

    public static TOut MatchWithValue1<TIn, TOut>(
        this ErrorOr<TIn> errorOr,
        Func<TIn, TOut> onSuccess,
        Func<IReadOnlyList<Error>, TOut> onError)
    {
        return errorOr.IsError ? onError(errorOr.Errors) : onSuccess(errorOr.Value);
    }

    public static async Task<TOut> MatchWithoutValueAsync1<TOut>(
       this ErrorOr<Success> errorOr,
       Func<Task<TOut>> onSuccess,
       Func<IReadOnlyList<Error>, Task<TOut>> onError)
    {
        return errorOr.IsError ? await onError(errorOr.Errors) : await onSuccess();
    }

    public static TOut MatchWithValue<TIn, TOut>(
        this ErrorOr<TIn> errorOr,
        Func<TIn, TOut> onSuccess,
        Func<IReadOnlyList<Error>, TOut> onError)
    {
        return onSuccess == null
            ? throw new ArgumentNullException(nameof(onSuccess))
            : onError == null
            ? throw new ArgumentNullException(nameof(onError))
            : errorOr.IsError
            ? onError(errorOr.Errors)
            : onSuccess(errorOr.Value);
    }

    public static async Task<TOut> MatchWithValueAsync<TIn, TOut>(
        this ErrorOr<TIn> errorOr,
        Func<TIn, Task<TOut>> onSuccess,
        Func<IReadOnlyList<Error>, Task<TOut>> onError)
    {
        return errorOr.IsError ? await onError(errorOr.Errors) : await onSuccess(errorOr.Value);
    }

    public static TOut MatchWithoutValue<TOut>(
        this ErrorOr<Success> errorOr,
        Func<TOut> onSuccess,
        Func<IReadOnlyList<Error>, TOut> onError)
    {
        return onSuccess == null
            ? throw new ArgumentNullException(nameof(onSuccess))
            : onError == null
            ? throw new ArgumentNullException(nameof(onError))
            : errorOr.IsError
            ? onError(errorOr.Errors)
            : onSuccess();
    }

    public static async Task<TOut> MatchWithoutValueAsync<TOut>(
        this ErrorOr<Success> errorOr,
        Func<Task<TOut>> onSuccess,
        Func<IReadOnlyList<Error>, Task<TOut>> onError)
    {
        return onSuccess == null
            ? throw new ArgumentNullException(nameof(onSuccess))
            : onError == null
            ? throw new ArgumentNullException(nameof(onError))
            : errorOr.IsError
            ? await onError(errorOr.Errors).ConfigureAwait(false)
            : await onSuccess().ConfigureAwait(false);
    }
}
