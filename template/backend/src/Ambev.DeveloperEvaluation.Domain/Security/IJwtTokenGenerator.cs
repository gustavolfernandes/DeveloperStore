namespace Ambev.DeveloperEvaluation.Domain.Security;

/// <summary>
/// Defines the contract for generating JWT authentication tokens for a user.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for the given user.
    /// </summary>
    /// <param name="user">The user the token is issued for.</param>
    /// <returns>A signed JWT token.</returns>
    string GenerateToken(IUser user);
}
