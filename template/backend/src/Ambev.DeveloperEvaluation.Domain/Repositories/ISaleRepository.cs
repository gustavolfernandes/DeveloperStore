using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository contract for the <see cref="Sale"/> aggregate.
/// </summary>
public interface ISaleRepository
{
    /// <summary>Persists a new sale.</summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sale (with its items) by its identifier.</summary>
    Task<Sale?> GetByIdAsync(SaleId id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sale (with its items) by its sale number.</summary>
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of sales together with the total number of records, applying the
    /// requested ordering expression (e.g. "saleDate desc, saleNumber asc").
    /// </summary>
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(
        int page,
        int size,
        string? order,
        CancellationToken cancellationToken = default);

    /// <summary>Persists changes made to an existing sale.</summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sale by its identifier. Returns false when it does not exist.</summary>
    Task<bool> DeleteAsync(SaleId id, CancellationToken cancellationToken = default);
}
