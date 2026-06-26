using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _dispatcher;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventDispatcher dispatcher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _dispatcher = dispatcher;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await new UpdateSaleValidator().ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(new SaleId(command.Id), cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        sale.Modify(
            command.SaleDate,
            new ExternalIdentity(command.CustomerId, command.CustomerName),
            new ExternalIdentity(command.BranchId, command.BranchName),
            command.Items.Select(i => (
                new ExternalIdentity(i.ProductId, i.ProductName),
                i.Quantity,
                i.UnitPrice)));

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _dispatcher.DispatchAsync(updated.DomainEvents, cancellationToken);
        updated.ClearDomainEvents();

        return _mapper.Map<SaleResult>(updated);
    }
}
