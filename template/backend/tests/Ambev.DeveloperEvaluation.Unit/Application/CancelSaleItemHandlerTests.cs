using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IDomainEventDispatcher _dispatcher = Substitute.For<IDomainEventDispatcher>();
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _handler = new CancelSaleItemHandler(_saleRepository, _mapper, _dispatcher);
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(new SaleResult());
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
    }

    private static Sale BuildSale() => Sale.Create("SALE-1", DateTime.UtcNow,
        new ExternalIdentity(Guid.NewGuid(), "Customer"),
        new ExternalIdentity(Guid.NewGuid(), "Branch"),
        [
            (new ExternalIdentity(Guid.NewGuid(), "P1"), 2, 100m),
            (new ExternalIdentity(Guid.NewGuid(), "P2"), 2, 50m)
        ]);

    [Fact(DisplayName = "Given an existing item When cancelling Then it is cancelled and events are dispatched")]
    public async Task Handle_ExistingItem_CancelsAndDispatches()
    {
        var sale = BuildSale();
        var item = sale.Items.First();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id.Value, item.Id.Value), CancellationToken.None);

        result.Should().NotBeNull();
        sale.Items.Single(i => i.Id == item.Id).IsCancelled.Should().BeTrue();
        await _dispatcher.Received(1).DispatchAsync(
            Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a missing sale When cancelling an item Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_Throws()
    {
        _saleRepository.GetByIdAsync(Arg.Any<SaleId>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
