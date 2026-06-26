using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>Raised when a single item of a sale is cancelled.</summary>
public sealed record SaleItemCancelledEvent(SaleId SaleId, string SaleNumber, SaleItemId SaleItemId, Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
