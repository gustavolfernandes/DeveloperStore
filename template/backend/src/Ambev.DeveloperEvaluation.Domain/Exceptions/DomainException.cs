namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Exception raised when a domain invariant or business rule is violated.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
