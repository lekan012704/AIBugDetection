using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Behaviors
{
    internal sealed class TransactionBehavior<TRequest, TResponse>(
    ILogger<TransactionBehavior<TRequest, TResponse>> logger, IUnitOfWork iUnitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<ITransactional>
    where TResponse : IErrorOr
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse? response = default;
            try
            {
                await iUnitOfWork.RetryOnExceptionAsync(async () =>
                {
                    logger.LogDebug($"Begin transaction: {typeof(TRequest).Name}.");
                    await iUnitOfWork.BeginTransactionAsync(cancellationToken);

                    response = await next();

                    await iUnitOfWork.CommitTransactionsAsync(cancellationToken);
                    logger.LogDebug($"End transaction: {typeof(TRequest).Name}.");
                });
            }
            catch (Exception e)
            {
                logger.LogError("Rollback transaction executed {@Request}.", typeof(TRequest).Name);
                await iUnitOfWork.RollbackTransactionAsync(cancellationToken);
                logger.LogError(e.Message, e.StackTrace);

                throw;
            }

            return response;
        }
    }
}
