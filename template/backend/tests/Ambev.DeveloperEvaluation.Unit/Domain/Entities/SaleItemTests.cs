using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    private static ExternalIdentity Product() => new(Guid.NewGuid(), "Product");

    [Theory(DisplayName = "Sale item applies the quantity-based discount and computes the total")]
    [InlineData(2, 100, 0, 200)]
    [InlineData(5, 100, 50, 450)]
    [InlineData(10, 100, 200, 800)]
    public void Constructor_AppliesDiscountAndTotal(int quantity, decimal unitPrice, decimal expectedDiscount, decimal expectedTotal)
    {
        var item = new SaleItem(Product(), quantity, unitPrice);

        item.Discount.Should().Be(expectedDiscount);
        item.Total.Should().Be(expectedTotal);
        item.GrossAmount.Should().Be(quantity * unitPrice);
    }

    [Fact(DisplayName = "Cancelling a sale item zeroes its monetary contribution")]
    public void Cancel_ZeroesAmounts()
    {
        var item = new SaleItem(Product(), 10, 100);

        item.Cancel();

        item.IsCancelled.Should().BeTrue();
        item.Discount.Should().Be(0);
        item.Total.Should().Be(0);
    }

    [Fact(DisplayName = "Creating an item with more than 20 units throws")]
    public void Constructor_ThrowsWhenQuantityExceedsMaximum()
    {
        Action act = () => new SaleItem(Product(), 21, 100);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Creating an item with a negative unit price throws")]
    public void Constructor_ThrowsWhenUnitPriceIsNegative()
    {
        Action act = () => new SaleItem(Product(), 1, -1);

        act.Should().Throw<DomainException>();
    }
}
