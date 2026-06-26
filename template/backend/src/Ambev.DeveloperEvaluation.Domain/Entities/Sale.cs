using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Aggregate root representing a sale record. Owns its <see cref="SaleItem"/> collection,
/// keeps the total amount consistent and raises domain events for the relevant business
/// operations (created, modified, cancelled, item cancelled).
/// </summary>
public class Sale : AggregateRoot<SaleId>
{
    private readonly List<SaleItem> _items = [];

    /// <summary>Human-readable, unique sale number.</summary>
    public string SaleNumber { get; private set; } = string.Empty;

    /// <summary>Date when the sale was made.</summary>
    public DateTime SaleDate { get; private set; }

    /// <summary>Customer the sale belongs to (External Identity + denormalized name).</summary>
    public ExternalIdentity Customer { get; private set; } = default!;

    /// <summary>Branch where the sale was made (External Identity + denormalized name).</summary>
    public ExternalIdentity Branch { get; private set; } = default!;

    /// <summary>Total amount of the sale (sum of the non-cancelled items' totals).</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Whether the whole sale has been cancelled.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Last update timestamp (UTC).</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>The items that compose this sale.</summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>Parameterless constructor required by EF Core.</summary>
    private Sale() { }

    private Sale(string saleNumber, DateTime saleDate, ExternalIdentity customer, ExternalIdentity branch)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required.");

        Id = SaleId.New();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        Customer = customer ?? throw new DomainException("Sale customer is required.");
        Branch = branch ?? throw new DomainException("Sale branch is required.");
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new sale with its items and raises a <see cref="SaleCreatedEvent"/>.
    /// </summary>
    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(ExternalIdentity Product, int Quantity, decimal UnitPrice)> items)
    {
        var sale = new Sale(saleNumber, saleDate, customer, branch);

        foreach (var (product, quantity, unitPrice) in items)
            sale.AddItemInternal(product, quantity, unitPrice);

        if (sale._items.Count == 0)
            throw new DomainException("A sale must contain at least one item.");

        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id, sale.SaleNumber));
        return sale;
    }

    /// <summary>Adds an item to an existing sale and raises a <see cref="SaleModifiedEvent"/>.</summary>
    public SaleItem AddItem(ExternalIdentity product, int quantity, decimal unitPrice)
    {
        EnsureNotCancelled();
        var item = AddItemInternal(product, quantity, unitPrice);
        MarkModified();
        return item;
    }

    /// <summary>
    /// Performs a full update of the sale (header information and items) and raises a
    /// single <see cref="SaleModifiedEvent"/>.
    /// </summary>
    public void Modify(
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(ExternalIdentity Product, int Quantity, decimal UnitPrice)> items)
    {
        EnsureNotCancelled();

        SaleDate = saleDate;
        Customer = customer ?? throw new DomainException("Sale customer is required.");
        Branch = branch ?? throw new DomainException("Sale branch is required.");

        _items.Clear();
        foreach (var (product, quantity, unitPrice) in items)
            AddItemInternal(product, quantity, unitPrice);

        if (_items.Count == 0)
            throw new DomainException("A sale must contain at least one item.");

        MarkModified();
    }

    /// <summary>Cancels a single item and raises a <see cref="SaleItemCancelledEvent"/>.</summary>
    public void CancelItem(SaleItemId saleItemId)
    {
        EnsureNotCancelled();

        var item = _items.FirstOrDefault(i => i.Id == saleItemId)
            ?? throw new DomainException($"Sale item {saleItemId} was not found in this sale.");

        if (item.IsCancelled)
            throw new DomainException("Sale item is already cancelled.");

        item.Cancel();
        Recalculate();
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleItemCancelledEvent(Id, SaleNumber, item.Id, item.Product.Id));
    }

    /// <summary>Cancels the whole sale and raises a <see cref="SaleCancelledEvent"/>.</summary>
    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Sale is already cancelled.");

        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleCancelledEvent(Id, SaleNumber));
    }

    private SaleItem AddItemInternal(ExternalIdentity product, int quantity, decimal unitPrice)
    {
        var item = new SaleItem(product, quantity, unitPrice);
        item.AttachTo(Id);
        _items.Add(item);
        Recalculate();
        return item;
    }

    private void MarkModified()
    {
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new DomainException("A cancelled sale cannot be modified.");
    }

    private void Recalculate() => TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.Total);
}
