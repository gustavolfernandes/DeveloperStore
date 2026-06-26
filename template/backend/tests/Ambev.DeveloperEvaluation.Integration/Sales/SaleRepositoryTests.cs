using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Infrastructure;
using Ambev.DeveloperEvaluation.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Integration tests for <see cref="SaleRepository"/> exercising the real EF Core model
/// (owned types, backing-field collection, relationships) against the in-memory provider.
/// Each test writes and reads through separate contexts that share the same in-memory
/// database, so the aggregate is genuinely re-materialized from the store.
/// </summary>
public class SaleRepositoryTests
{
    private readonly string _databaseName = $"sales-{Guid.NewGuid()}";

    private DefaultContext CreateContext() =>
        new(new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private static Sale BuildSale(string saleNumber, int quantity = 5, decimal unitPrice = 100m) =>
        Sale.Create(saleNumber, DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Customer"),
            new ExternalIdentity(Guid.NewGuid(), "Branch"),
            [(new ExternalIdentity(Guid.NewGuid(), "Product"), quantity, unitPrice)]);

    private async Task SeedAsync(params Sale[] sales)
    {
        await using var context = CreateContext();
        var repository = new SaleRepository(context);
        foreach (var sale in sales)
            await repository.CreateAsync(sale);
    }

    [Fact(DisplayName = "CreateAsync persists the sale and GetByIdAsync re-materializes it with its items")]
    public async Task CreateAndGetById_RoundTrips()
    {
        var sale = BuildSale("SALE-100");
        await SeedAsync(sale);

        await using var context = CreateContext();
        var loaded = await new SaleRepository(context).GetByIdAsync(sale.Id);

        loaded.Should().NotBeNull();
        loaded!.SaleNumber.Should().Be("SALE-100");
        loaded.Customer.Name.Should().Be("Customer");
        loaded.Branch.Name.Should().Be("Branch");
        loaded.Items.Should().HaveCount(1);
        loaded.Items.First().Product.Name.Should().Be("Product");
        loaded.Items.First().Total.Should().Be(450m);
        loaded.TotalAmount.Should().Be(450m);
    }

    [Fact(DisplayName = "GetBySaleNumberAsync finds the sale by its number")]
    public async Task GetBySaleNumber_ReturnsSale()
    {
        await SeedAsync(BuildSale("SALE-XYZ"));

        await using var context = CreateContext();
        var loaded = await new SaleRepository(context).GetBySaleNumberAsync("SALE-XYZ");

        loaded.Should().NotBeNull();
        loaded!.SaleNumber.Should().Be("SALE-XYZ");
    }

    [Fact(DisplayName = "ListAsync paginates and orders by the requested expression")]
    public async Task ListAsync_OrdersAndPaginates()
    {
        await SeedAsync(BuildSale("SALE-003"), BuildSale("SALE-001"), BuildSale("SALE-002"));

        await using var context = CreateContext();
        var (items, totalCount) = await new SaleRepository(context)
            .ListAsync(page: 1, size: 2, order: "saleNumber asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
        items.Select(s => s.SaleNumber).Should().ContainInOrder("SALE-001", "SALE-002");
    }

    [Fact(DisplayName = "DeleteAsync removes the sale and its items")]
    public async Task DeleteAsync_RemovesSale()
    {
        var sale = BuildSale("SALE-DEL");
        await SeedAsync(sale);

        await using (var context = CreateContext())
        {
            var deleted = await new SaleRepository(context).DeleteAsync(sale.Id);
            deleted.Should().BeTrue();
        }

        await using var verifyContext = CreateContext();
        (await new SaleRepository(verifyContext).GetByIdAsync(sale.Id)).Should().BeNull();
    }

    [Fact(DisplayName = "DeleteAsync returns false when the sale does not exist")]
    public async Task DeleteAsync_ReturnsFalseWhenMissing()
    {
        await using var context = CreateContext();

        var deleted = await new SaleRepository(context).DeleteAsync(SaleId.New());

        deleted.Should().BeFalse();
    }
}
