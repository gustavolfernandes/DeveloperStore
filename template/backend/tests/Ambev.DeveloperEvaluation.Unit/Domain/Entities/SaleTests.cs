using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    private static ExternalIdentity Ext() => new(Guid.NewGuid(), "Reference");

    [Fact(DisplayName = "Create builds the sale, sums the total and raises SaleCreatedEvent")]
    public void Create_BuildsSaleAndRaisesEvent()
    {
        var sale = Sale.Create("SALE-1", DateTime.UtcNow, Ext(), Ext(),
        [
            (Ext(), 5, 100m),
            (Ext(), 2, 50m)
        ]);

        sale.Id.Value.Should().NotBeEmpty();
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(550m);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Fact(DisplayName = "Create with no items throws")]
    public void Create_ThrowsWhenNoItems()
    {
        Action act = () => Sale.Create("SALE-1", DateTime.UtcNow, Ext(), Ext(), []);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Create with an item above 20 units throws")]
    public void Create_ThrowsWhenItemExceedsMaximum()
    {
        Action act = () => Sale.Create("SALE-1", DateTime.UtcNow, Ext(), Ext(), [(Ext(), 21, 10m)]);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "AddItem recalculates the total and raises SaleModifiedEvent")]
    public void AddItem_RecalculatesAndRaisesModified()
    {
        var sale = SaleTestData.GenerateValidSale(itemQuantity: 1, unitPrice: 100m);
        sale.ClearDomainEvents();

        sale.AddItem(Ext(), 4, 100m);

        sale.TotalAmount.Should().Be(460m);
        sale.Items.Should().HaveCount(2);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleModifiedEvent);
    }

    [Fact(DisplayName = "CancelItem excludes the item from the total and raises SaleItemCancelledEvent")]
    public void CancelItem_RecalculatesAndRaisesEvent()
    {
        var sale = Sale.Create("SALE-1", DateTime.UtcNow, Ext(), Ext(),
        [
            (Ext(), 2, 100m),
            (Ext(), 2, 50m)
        ]);
        sale.ClearDomainEvents();
        var itemToCancel = sale.Items.First();

        sale.CancelItem(itemToCancel.Id);

        sale.TotalAmount.Should().Be(100m);
        sale.Items.Single(i => i.Id == itemToCancel.Id).IsCancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleItemCancelledEvent);
    }

    [Fact(DisplayName = "CancelItem with an unknown id throws")]
    public void CancelItem_ThrowsWhenItemNotFound()
    {
        var sale = SaleTestData.GenerateValidSale();

        Action act = () => sale.CancelItem(SaleItemId.New());

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Cancel marks the sale cancelled and raises SaleCancelledEvent")]
    public void Cancel_RaisesEvent()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.ClearDomainEvents();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "A cancelled sale cannot be modified")]
    public void Cancel_PreventsFurtherModification()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();

        sale.Invoking(s => s.AddItem(Ext(), 1, 10m)).Should().Throw<DomainException>();
        sale.Invoking(s => s.Cancel()).Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Modify replaces the items, recomputes the total and raises SaleModifiedEvent")]
    public void Modify_ReplacesItemsAndRaisesEvent()
    {
        var sale = SaleTestData.GenerateValidSale(itemQuantity: 1, unitPrice: 100m);
        sale.ClearDomainEvents();

        sale.Modify(DateTime.UtcNow, Ext(), Ext(), [(Ext(), 10, 100m)]);

        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(800m);
        sale.DomainEvents.Should().ContainSingle(e => e is SaleModifiedEvent);
    }
}
