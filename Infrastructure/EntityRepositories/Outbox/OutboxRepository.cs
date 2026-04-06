using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.Outbox;
using Domain.Application.Entities.Outbox;
using Infrastructure.GenericRepository;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;
using System.Text.Json;

namespace Infrastructure.EntityRepositories.Outbox
{
    public class OutboxRepository : RepositoryAsync<OutboxMessage, Guid>, IOutboxRepository
    {
        private readonly OutboxOptions _outboxOptions;
        private readonly IDateTimeProvider _iDateTimeProvider;
        private const string DefaultUserName = "System Application";
        private readonly IOutboxRepository outboxRepository;
        public OutboxRepository(IUnitOfWork context, IDateTimeProvider iDateTimeProvider, IOptions<OutboxOptions> outboxOptions, IOutboxRepository outboxRepository)
                : base(context)
        {
            _outboxOptions = outboxOptions.Value;
            _iDateTimeProvider = iDateTimeProvider;
            this.outboxRepository = outboxRepository;
        }

        public OutboxMessage AddToOutbox<T>(T message)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CreatedAt = _iDateTimeProvider.UtcNow,
                Type = typeof(T).FullName ?? typeof(T).Name,
                Content = JsonSerializer.Serialize(message),
                Status = OutboxMessageStatus.Pending.ToString(),
                RetryCount = 0,
                UpdatedAt = null,
                UpdatedBy = null,
                CreatedBy = DefaultUserName,
            };

            return outboxMessage;
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessages(CancellationToken cancellationToken = default)
        {
            return await outboxRepository.Get(m => m.Status == OutboxMessageStatus.Pending.ToString())
                        .Take(_outboxOptions.BatchSize)
                        .ToListAsync(cancellationToken);
        }

        public OutboxMessage MarkAsProcessing(OutboxMessage message)
        {
            var record = new OutboxMessage(
                            message.Id,
                            message.CreatedAt,
                            message.Type,
                            message.Content,
                            OutboxMessageStatus.Processing.ToString(),
                            retryCount: message.RetryCount,
                            _iDateTimeProvider.UtcNow,
                            DefaultUserName,
                            null
        )
            { CreatedBy = DefaultUserName };
            return record;
        }

        public OutboxMessage MarkAsProcessed(OutboxMessage message)
        {
            var count = message.RetryCount;
            var record = new OutboxMessage(
                            message.Id,
                            message.CreatedAt,
                            message.Type,
                            message.Content,
                            OutboxMessageStatus.Completed.ToString(),
                            retryCount: count += 1,
                            _iDateTimeProvider.UtcNow,
                            DefaultUserName,
                            null
        )
            { CreatedBy = DefaultUserName };

            return record;
        }

        public OutboxMessage MarkAsFailed(OutboxMessage message, string errorDetails)
        {
            var count = message.RetryCount;
            var record = new OutboxMessage(
                            message.Id,
                            message.CreatedAt,
                            message.Type,
                            message.Content,
                            OutboxMessageStatus.Failed.ToString(),
                            retryCount: count += 1,
                            _iDateTimeProvider.UtcNow,
                            DefaultUserName,
                            errorDetails
        )
            { CreatedBy = DefaultUserName };

            return record;
        }

        public async Task<IEnumerable<OutboxMessage>> GetAuditTrailAsync(OutboxMessageFilter filter, CancellationToken cancellationToken = default)
        {
            var query = outboxRepository.GetAll();

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(a => a.Type == filter.Type);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}