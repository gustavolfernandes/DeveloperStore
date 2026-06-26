using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// A single line in a <see cref="Sale"/>. Belongs to the Sale aggregate and is never
/// modified outside of it. Applies the quantity-based discount rules on construction
/// and whenever its quantity changes.
/// </summary>
public class SaleItem : BaseEntity<SaleItemId>
{
    /// <summary>Identifier of the owning sale.</summary>
    public SaleId SaleId { get; private set; }

    /// <summary>Referenced product (External Identity + denormalized title).</summary>
    public ExternalIdentity Product { get; private set; } = default!;

    /// <summary>Number of identical units of the product.</summary>
    public int Quantity { get; private set; }

    /// <summary>Unit price of the product at the moment of the sale.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Discount rate applied to this item (0, 0.10 or 0.20).</summary>
    public decimal DiscountRate { get; private set; }

    /// <summary>Monetary discount applied to this item.</summary>
    public decimal Discount { get; private set; }

    /// <summary>Net total for this item (gross minus discount).</summary>
    public decimal Total { get; private set; }

    /// <summary>Whether this item has been cancelled.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Gross amount for this item before discount.</summary>
    public decimal GrossAmount => UnitPrice * Quantity;

    /// <summary>Parameterless constructor required by EF Core.</summary>
    private SaleItem() { }

    public SaleItem(ExternalIdentity product, int quantity, decimal unitPrice)
    {
        Id = SaleItemId.New();
        Product = product ?? throw new ArgumentNullException(nameof(product));

        if (unitPrice < 0)
            throw new Exceptions.DomainException("Unit price cannot be negative.");

        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Changes the quantity, re-applying the discount rules and recomputing the totals.
    /// </summary>
    public void SetQuantity(int quantity)
    {
        DiscountRate = SaleDiscountPolicy.GetDiscountRate(quantity);
        Quantity = quantity;
        Recalculate();
    }

    /// <summary>Marks this item as cancelled and zeroes its monetary contribution.</summary>
    public void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        Recalculate();
    }

    internal void AttachTo(SaleId saleId) => SaleId = saleId;

    private void Recalculate()
    {
        if (IsCancelled)
        {
            Discount = 0m;
            Total = 0m;
            return;
        }

        Discount = GrossAmount * DiscountRate;
        Total = GrossAmount - Discount;
    }
}
