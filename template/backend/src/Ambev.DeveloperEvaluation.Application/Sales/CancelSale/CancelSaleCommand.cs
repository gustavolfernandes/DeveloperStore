using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>Command to cancel a whole sale.</summary>
public record CancelSaleCommand(Guid Id) : IRequest<SaleResult>;
