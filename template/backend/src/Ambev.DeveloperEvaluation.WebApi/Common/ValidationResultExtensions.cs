using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation.Results;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Helpers to convert FluentValidation results into the standard API response envelope.
/// </summary>
public static class ValidationResultExtensions
{
    public static ApiResponse ToApiResponse(this ValidationResult validationResult)
    {
        return new ApiResponse
        {
            Success = false,
            Message = "Validation Failed",
            Errors = validationResult.Errors.Select(error => (ValidationErrorDetail)error)
        };
    }
}
