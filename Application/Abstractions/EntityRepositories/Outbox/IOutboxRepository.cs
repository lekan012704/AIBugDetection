using Application.Abstractions.GenericRepository;
using Domain.Application.Entities.Outbox;

namespace Application.Abstractions.EntityRepositories.Outbox
{
    public interface IOutboxRepository : IRepositoryAsync<OutboxMessage, Guid>
    {
        OutboxMessage AddToOutbox<T>(T message);
        Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessages(CancellationToken cancellationToken = default);
        OutboxMessage MarkAsFailed(OutboxMessage message, string errorDetails);
        OutboxMessage MarkAsProcessing(OutboxMessage message);
        OutboxMessage MarkAsProcessed(OutboxMessage message);
        Task<IEnumerable<OutboxMessage>> GetAuditTrailAsync(OutboxMessageFilter filter, CancellationToken cancellationToken = default);
    }
}
