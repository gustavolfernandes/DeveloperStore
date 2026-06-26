using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Endpoints;

/// <summary>
/// Minimal API endpoints for authentication.
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/", AuthenticateAsync)
            .WithName("AuthenticateUser")
            .Produces<ApiResponseWithData<AuthenticateUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static async Task<IResult> AuthenticateAsync(
        [FromBody] AuthenticateUserRequest request,
        ISender mediator,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var validation = await new AuthenticateUserRequestValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.BadRequest(validation.ToApiResponse());

        var command = mapper.Map<AuthenticateUserCommand>(request);
        var result = await mediator.Send(command, cancellationToken);

        return Results.Ok(new ApiResponseWithData<AuthenticateUserResponse>
        {
            Success = true,
            Message = "User authenticated successfully",
            Data = mapper.Map<AuthenticateUserResponse>(result)
        });
    }
}
