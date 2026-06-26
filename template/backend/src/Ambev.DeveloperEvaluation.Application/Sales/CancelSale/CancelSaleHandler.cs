using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _dispatcher;

    public CancelSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventDispatcher dispatcher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _dispatcher = dispatcher;
    }

    public async Task<SaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(new SaleId(request.Id), cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.Id} not found.");

        sale.Cancel();
        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _dispatcher.DispatchAsync(updated.DomainEvents, cancellationToken);
        updated.ClearDomainEvents();

        return _mapper.Map<SaleResult>(updated);
    }
}
