namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Marker interface for domain events raised by aggregate roots.
/// The Domain layer stays framework-agnostic; dispatching/publishing is the
/// responsibility of the Application layer.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The moment the event occurred (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}
