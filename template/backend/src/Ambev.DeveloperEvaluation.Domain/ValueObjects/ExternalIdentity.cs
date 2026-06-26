using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Value object implementing the <c>External Identities</c> pattern with denormalization:
/// it carries the identifier of an entity owned by another domain (customer, branch, product)
/// together with a denormalized human-readable description.
/// </summary>
public sealed record ExternalIdentity
{
    /// <summary>Identifier of the referenced entity in its owning domain.</summary>
    public Guid Id { get; init; }

    /// <summary>Denormalized description (name/title) of the referenced entity.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Parameterless constructor required by EF Core for owned-type materialization.</summary>
    private ExternalIdentity() { }

    public ExternalIdentity(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("External identity id cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("External identity name cannot be empty.");

        Id = id;
        Name = name;
    }
}
