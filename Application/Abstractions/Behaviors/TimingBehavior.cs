using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Application.Abstractions.Behaviors
{
    internal sealed class TimingBehavior<TRequest, TResponse>(
    ILogger<TimingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : IErrorOr
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return await LogExecutionDuration(() => next());
        }
        private async Task<TResult> LogExecutionDuration<TResult>(Func<Task<TResult>> func, [CallerMemberName] string member = "")
        {
            var stopwatch = Stopwatch.StartNew();
            TResult result;
            try
            {
                result = await func();
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation(message: $"Execution duration for {member} takes {stopwatch.ElapsedMilliseconds}ms");
            }
            return result;
        }
    }
}
