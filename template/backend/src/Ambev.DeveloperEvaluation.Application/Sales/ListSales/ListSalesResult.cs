using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>A page of sales together with paging metadata.</summary>
public class ListSalesResult
{
    public IReadOnlyList<SaleResult> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages => Size > 0 ? (int)Math.Ceiling(TotalCount / (double)Size) : 0;
}
