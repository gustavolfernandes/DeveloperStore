using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _dispatcher;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventDispatcher dispatcher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _dispatcher = dispatcher;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(new SaleId(request.SaleId), cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found.");

        sale.CancelItem(new SaleItemId(request.ItemId));
        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _dispatcher.DispatchAsync(updated.DomainEvents, cancellationToken);
        updated.ClearDomainEvents();

        return _mapper.Map<SaleResult>(updated);
    }
}
