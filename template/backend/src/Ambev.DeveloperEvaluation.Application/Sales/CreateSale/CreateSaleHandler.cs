using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _dispatcher;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventDispatcher dispatcher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _dispatcher = dispatcher;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await new CreateSaleValidator().ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existing = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"A sale with number {command.SaleNumber} already exists.");

        var sale = Sale.Create(
            command.SaleNumber,
            command.SaleDate,
            new ExternalIdentity(command.CustomerId, command.CustomerName),
            new ExternalIdentity(command.BranchId, command.BranchName),
            command.Items.Select(i => (
                new ExternalIdentity(i.ProductId, i.ProductName),
                i.Quantity,
                i.UnitPrice)));

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _dispatcher.DispatchAsync(created.DomainEvents, cancellationToken);
        created.ClearDomainEvents();

        return _mapper.Map<SaleResult>(created);
    }
}
