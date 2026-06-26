using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Centralizes test data generation for the Sale aggregate using Bogus.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static ExternalIdentity GenerateExternalIdentity()
        => new(Guid.NewGuid(), Faker.Company.CompanyName());

    public static (ExternalIdentity Product, int Quantity, decimal UnitPrice) GenerateItem(
        int quantity = 1, decimal unitPrice = 10m)
        => (new ExternalIdentity(Guid.NewGuid(), Faker.Commerce.ProductName()), quantity, unitPrice);

    /// <summary>Creates a valid sale with a single item (quantity defaults to 1, no discount).</summary>
    public static Sale GenerateValidSale(int itemQuantity = 1, decimal unitPrice = 10m)
        => Sale.Create(
            $"SALE-{Faker.Random.Number(1000, 9999)}",
            DateTime.UtcNow,
            GenerateExternalIdentity(),
            GenerateExternalIdentity(),
            [GenerateItem(itemQuantity, unitPrice)]);
}
