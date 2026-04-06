using Application.Abstractions.Authentication.Custom;
using Application.Abstractions.Data;
using Domain.Application.Entities.Audits;
using Domain.Application.Entities.BugDetection;
using Domain.Application.Entities.Outbox;
using Domain.Application.Entities.Permissions;
using Domain.Application.Entities.UserPermissions;
using Domain.Application.Entities.Users;
using Infrastructure.Helpers;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using SharedKernel;
using System.Data;
using System.Reflection;

namespace Infrastructure.Database;


public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext) : DbContext(options), IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public DbSet<User> Users { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<Audit> AuditLogs { get; set; }
    public DbSet<CodeAnalysisSession> CodeAnalysisSessions { get; set; }
    public DbSet<CodeIssue> CodeIssues { get; set; }
    public DbSet<AnalysisConversation> AnalysisConversations { get; set; }
    public DbSet<BugReport> BugReports { get; set; }
    public DbSet<BugItem> BugItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.Default);

       
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

       
        var domainAssembly = Assembly.Load("Domain");
        modelBuilder.ApplyConfigurationsFromAssembly(domainAssembly);

       
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

           
            entity.HasIndex(u => u.KeycloakId).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.RowVersion).IsRowVersion();
        });
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> CommitTransactionsAsync(CancellationToken cancellationToken)
    {
        int result = 0;
        try
        {
            result = await SaveChangesAsync(cancellationToken);
            _currentTransaction?.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackTransactionAsync(cancellationToken);
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
        return result;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RetryOnExceptionAsync(Func<Task> func)
    {
        await Database.CreateExecutionStrategy().ExecuteAsync(func);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    public async Task<int> PersistChangesAsync(
        bool logAuditTrail = false,
        bool raisedEvent = false,
        bool raisedEventAsOutBoxMessage = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (logAuditTrail)
                LogAuditTrail();

            if (raisedEvent)
                await PublishDomainEventsAsync();

            if (raisedEventAsOutBoxMessage)
                AddDomainEventsAsOutboxMessages();

            return await CommitTransactionsAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<EventEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent);
    }

    private void AddDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker
            .Entries<EventEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                DateTime.UtcNow,
                domainEvent.GetType().Name,
                JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings),
                OutboxMessageStatus.Pending.ToString(),
                retryCount: 0,
                null, null, null)
            { CreatedBy = "System Application" })
            .ToList();

        AddRange(outboxMessages);
    }

    private void LogAuditTrail()
    {
        // ✅ Safe — never throws even if user context is unavailable
        string userName;
        try
        {
            userName = userContext?.UserName ?? "System";
        }
        catch
        {
            userName = "System";
        }

        if (string.IsNullOrWhiteSpace(userName))
            userName = "System";

        ChangeTracker.DetectChanges();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = dateTimeProvider.UtcNow;
                    entry.Entity.CreatedBy = userName;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = dateTimeProvider.UtcNow;
                    entry.Entity.UpdatedBy = userName;
                    break;
            }
        }

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Audit ||
                entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(
                entry,
                entry.Entity.GetType().Name,
                userName);

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (property.CurrentValue != null)
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        if (property.CurrentValue != null)
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        if (property.OriginalValue != null)
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            if (property.OriginalValue != null)
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                            if (property.CurrentValue != null)
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries)
            AuditLogs.Add(auditEntry.ToAudit());
    }
}