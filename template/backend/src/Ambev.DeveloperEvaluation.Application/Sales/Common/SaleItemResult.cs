namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>Representation of a single sale item returned by the API.</summary>
public class SaleItemResult
{
    public Guid Id { get; set; }
    public ExternalIdentityDto Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }
}
