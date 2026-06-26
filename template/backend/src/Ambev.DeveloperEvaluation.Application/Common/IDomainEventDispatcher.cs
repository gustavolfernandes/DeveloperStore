using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Application.Common;

/// <summary>
/// Dispatches the domain events raised by an aggregate, decoupling the aggregates that
/// produce events from the side effects that react to them.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
