using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Endpoints;

/// <summary>
/// Minimal API endpoints for user management.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/", CreateAsync)
            .WithName("CreateUser")
            .Produces<ApiResponseWithData<CreateUserResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetUser")
            .RequireAuthorization()
            .Produces<ApiResponseWithData<GetUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteUser")
            .RequireAuthorization()
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateUserRequest request,
        ISender mediator,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var validation = await new CreateUserRequestValidator().ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.BadRequest(validation.ToApiResponse());

        var command = mapper.Map<CreateUserCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
        var data = mapper.Map<CreateUserResponse>(result);

        return Results.Created($"/api/users/{data.Id}", new ApiResponseWithData<CreateUserResponse>
        {
            Success = true,
            Message = "User created successfully",
            Data = data
        });
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        ISender mediator,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserCommand(id), cancellationToken);

        return Results.Ok(new ApiResponseWithData<GetUserResponse>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = mapper.Map<GetUserResponse>(result)
        });
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(id), cancellationToken);

        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "User deleted successfully"
        });
    }
}
