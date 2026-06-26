namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>Full representation of a sale returned by the API.</summary>
public class SaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityDto Customer { get; set; } = new();
    public ExternalIdentityDto Branch { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SaleItemResult> Items { get; set; } = [];
}
