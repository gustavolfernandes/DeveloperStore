using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>Query to retrieve a paginated, ordered list of sales.</summary>
public class ListSalesQuery : IRequest<ListSalesResult>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    /// <summary>Ordering expression, e.g. "saleDate desc, saleNumber asc".</summary>
    public string? Order { get; set; }
}
