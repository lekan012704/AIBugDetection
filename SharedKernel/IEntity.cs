using SharedKernel;

namespace Domain.Abstractions
{
    public interface IEntity
    {
        IReadOnlyList<IDomainEvent> GetDomainEvents();
        void ClearDomainEvents();
        void RaiseDomainEvent(IDomainEvent domainEvent);
    }
}
