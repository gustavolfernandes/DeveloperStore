using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

public class SaleDiscountPolicyTests
{
    [Theory(DisplayName = "GetDiscountRate returns the correct rate for each quantity tier")]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(20, 20)]
    public void GetDiscountRate_ReturnsExpectedRate(int quantity, int expectedPercent)
    {
        var rate = SaleDiscountPolicy.GetDiscountRate(quantity);

        rate.Should().Be(expectedPercent / 100m);
    }

    [Theory(DisplayName = "GetDiscountRate throws for quantities outside the allowed range")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(100)]
    public void GetDiscountRate_ThrowsForInvalidQuantity(int quantity)
    {
        Action act = () => SaleDiscountPolicy.GetDiscountRate(quantity);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Selling more than 20 identical items is not allowed")]
    public void EnsureQuantityIsAllowed_ThrowsAboveMaximum()
    {
        Action act = () => SaleDiscountPolicy.EnsureQuantityIsAllowed(SaleDiscountPolicy.MaxQuantityPerItem + 1);

        act.Should().Throw<DomainException>()
            .WithMessage("*more than 20*");
    }
}
