namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>Denormalized reference to an entity owned by another domain.</summary>
public class ExternalIdentityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
