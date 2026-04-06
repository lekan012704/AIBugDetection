using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data
{
    public interface IUnitOfWork
    {
        DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

        Task BeginTransactionAsync(CancellationToken cancellationToken);
        Task<int> CommitTransactionsAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        Task RetryOnExceptionAsync(Func<Task> func);
        Task<int> PersistChangesAsync(bool logAuditTrail = false, bool raisedEvent = false, bool raisedEventAsOutBoxMessage = false, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
