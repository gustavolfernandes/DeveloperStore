namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Base class for aggregate roots. Adds support for collecting domain events
/// that the Application layer can dispatch after the aggregate is persisted.
/// </summary>
public abstract class AggregateRoot<TId> : BaseEntity<TId>
    where TId : struct, IComparable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate since it was loaded/created.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
