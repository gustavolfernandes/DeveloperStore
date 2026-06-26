using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>Command to perform a full update of an existing sale.</summary>
public class UpdateSaleCommand : IRequest<SaleResult>
{
    public Guid Id { get; set; }
    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public List<UpdateSaleItemCommand> Items { get; set; } = [];
}

/// <summary>A single item of an <see cref="UpdateSaleCommand"/>.</summary>
public class UpdateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
