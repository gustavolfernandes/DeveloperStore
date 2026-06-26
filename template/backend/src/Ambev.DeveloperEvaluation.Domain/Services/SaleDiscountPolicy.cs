using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Encapsulates the quantity-based discount business rules for sale items:
/// <list type="bullet">
/// <item>Quantity below 4: no discount.</item>
/// <item>Quantity from 4 to 9: 10% discount.</item>
/// <item>Quantity from 10 to 20: 20% discount.</item>
/// <item>Quantity above 20: not allowed.</item>
/// </list>
/// </summary>
public static class SaleDiscountPolicy
{
    /// <summary>Minimum quantity required to be eligible for any discount.</summary>
    public const int MinQuantityForDiscount = 4;

    /// <summary>Quantity threshold (inclusive) where the higher discount tier starts.</summary>
    public const int HigherTierThreshold = 10;

    /// <summary>Maximum quantity allowed per item.</summary>
    public const int MaxQuantityPerItem = 20;

    /// <summary>
    /// Validates that the given quantity is within the allowed range (1..20).
    /// </summary>
    /// <exception cref="DomainException">Thrown when the quantity is invalid.</exception>
    public static void EnsureQuantityIsAllowed(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Sale item quantity must be at least 1.");

        if (quantity > MaxQuantityPerItem)
            throw new DomainException($"It is not possible to sell more than {MaxQuantityPerItem} identical items.");
    }

    /// <summary>
    /// Returns the discount rate (0, 0.10 or 0.20) that applies to the given quantity.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the quantity exceeds the allowed maximum.</exception>
    public static decimal GetDiscountRate(int quantity)
    {
        EnsureQuantityIsAllowed(quantity);

        if (quantity >= HigherTierThreshold)
            return 0.20m;

        if (quantity >= MinQuantityForDiscount)
            return 0.10m;

        return 0m;
    }
}
