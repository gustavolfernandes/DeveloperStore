using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>Query to retrieve a single sale by its identifier.</summary>
public record GetSaleQuery(Guid Id) : IRequest<SaleResult>;
