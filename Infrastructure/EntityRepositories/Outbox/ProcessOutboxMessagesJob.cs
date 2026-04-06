using Application.Abstractions.Data;
using Application.Abstractions.EntityRepositories.Outbox;
using Application.Dapper;
using Dapper;
using Domain.Application.Entities.Outbox;
using InterpolatedSql.Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using SharedKernel;
using System.Data;
using System.Reflection;

namespace Infrastructure.EntityRepositories.Outbox
{
    [DisallowConcurrentExecution]
    internal sealed class ProcessOutboxMessagesJob : IJob
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IDapperFactory _iDapperFactory;
        private readonly IPublisher _publisher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly OutboxOptions _outboxOptions;
        private readonly ILogger<ProcessOutboxMessagesJob> _logger;
        private readonly IOutboxRepository _iOutboxRepository;
        private readonly Assembly _integrationEventsAssembly;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessOutboxMessagesJob(
            IDapperFactory iDapperFactory,
            IPublisher publisher,
            IDateTimeProvider dateTimeProvider,
            IOptions<OutboxOptions> outboxOptions,
            IOutboxRepository iOutboxRepository,
            ILogger<ProcessOutboxMessagesJob> logger,
            Assembly integrationEventsAssembly,
            IUnitOfWork unitOfWork)
        {
            _iDapperFactory = iDapperFactory;
            _publisher = publisher;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _outboxOptions = outboxOptions.Value;
            _iOutboxRepository = iOutboxRepository;
            _integrationEventsAssembly = integrationEventsAssembly;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Beginning to process outbox messages");

            try
            {
                using IDbConnection connection = _iDapperFactory.CreateConnection();
                using IDbTransaction transaction = connection.BeginTransaction();

                await _unitOfWork.BeginTransactionAsync(context.CancellationToken);

                // Get unprocessed messages using your repository method
                IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

                if (outboxMessages.Count == 0)
                {
                    _logger.LogInformation("No unprocessed outbox messages found");
                    return;
                }

                // Process each outbox message
                foreach (OutboxMessageResponse outboxMessage in outboxMessages)
                {
                    Exception? exception = null;
                    OutboxMessage? updatedMessage;

                    try
                    {
                        // Get the message type from the assembly
                        var messageType = _integrationEventsAssembly.GetType(outboxMessage.type) ?? throw new InvalidOperationException($"Could not find type {outboxMessage.type} in assembly");

                        var deserializedMessage = JsonSerializer.Create().Deserialize(
                            new StringReader(outboxMessage.Content), messageType);

                        if (deserializedMessage == null)
                        {
                            throw new InvalidOperationException($"Failed to deserialize message of type {outboxMessage.type}");
                        }

                        IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                            outboxMessage.Content,
                            JsonSerializerSettings)!;

                        if (domainEvent == null)
                        {
                            throw new InvalidOperationException($"Failed to deserialize message of type {outboxMessage.type}");
                        }

                        updatedMessage = await GetOutboxMessagesByIdAsync(outboxMessage.Id, context.CancellationToken);
                        if (updatedMessage is null)
                        {
                            _logger.LogWarning("Outbox message {MessageId} not found", outboxMessage.Id);
                            continue;
                        }

                        // Mark as processing using our repository method
                        updatedMessage = _iOutboxRepository.MarkAsProcessing(updatedMessage);

                        // Publish the event
                        await _publisher.Publish(domainEvent, context.CancellationToken);

                        // Mark as processed using our repository method
                        updatedMessage = _iOutboxRepository.MarkAsProcessed(updatedMessage);

                        _logger.LogDebug("Successfully processed outbox message {MessageId}", outboxMessage.Id);
                    }
                    catch (Exception caughtException)
                    {
                        _logger.LogError(
                            caughtException,
                            "Exception while processing outbox message {MessageId}",
                            outboxMessage.Id);

                        exception = caughtException;

                        // Mark as failed using our repository method
                        updatedMessage = await GetOutboxMessagesByIdAsync(outboxMessage.Id, context.CancellationToken);
                        if (updatedMessage is null)
                        {
                            _logger.LogWarning("Outbox message {MessageId} not found for marking as failed", outboxMessage.Id);
                            continue;
                        }
                        updatedMessage = _iOutboxRepository.MarkAsFailed(updatedMessage, exception.ToString());
                    }

                    // Update the message in the database
                    await _iOutboxRepository.UpdateAsync(updatedMessage, context.CancellationToken);
                }


                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                await _unitOfWork.CommitTransactionsAsync(context.CancellationToken);

                _logger.LogInformation("Completed processing {Count} outbox messages", outboxMessages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                throw;
            }
        }

        private async Task<OutboxMessage?> GetOutboxMessagesByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            OutboxMessage? outboxMessage = await _iOutboxRepository.GetByIdAsync(id, cancellationToken);

            return outboxMessage ?? null;
        }

        private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
            IDbConnection connection,
            IDbTransaction transaction)
        {

            string sql = $"""
                      SELECT TOP {_outboxOptions.BatchSize}
                          Id, Content, Type
                      FROM [SelfService].[OutboxMessages]
                      WHERE Status = @Pending
                      """;

            IEnumerable<OutboxMessageResponse> outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(
                sql,
                new
                {
                    Status = "Pending"
                },
                transaction: transaction);

            return [.. outboxMessages];
        }

        private async Task UpdateOutboxMessageAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            OutboxMessageResponse outboxMessage,
            Exception? exception)
        {
            const string sql = @"
            UPDATE [SelfService].[OutboxMessages]
            SET UpdatedOn = @ProcessedOnUtc,
                LastUpdatedBy = @LastUpdatedBy,
                error = @Error
            WHERE id = @Id";

            await connection.ExecuteAsync(
                sql,
                new
                {
                    outboxMessage.Id,
                    ProcessedOnUtc = _dateTimeProvider.UtcNow,
                    LastUpdatedBy = "System",
                    Error = exception?.ToString()
                },
                transaction: transaction);

            _logger.LogDebug("Successfully processed outbox message {MessageId}", outboxMessage.Id);
        }

        internal sealed record OutboxMessageResponse(Guid Id, string Content, string type);
    }
}