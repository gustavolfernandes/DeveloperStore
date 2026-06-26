using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Endpoints;

/// <summary>Minimal API endpoints for the Sales resource.</summary>
public static class SaleEndpoints
{
    public static IEndpointRouteBuilder MapSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales").WithTags("Sales").RequireAuthorization();

        group.MapPost("/", CreateAsync).WithName("CreateSale")
            .Produces<ApiResponseWithData<SaleResult>>(StatusCodes.Status201Created)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/", ListAsync).WithName("ListSales")
            .Produces<PaginatedResponse<SaleResult>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetAsync).WithName("GetSale")
            .Produces<ApiResponseWithData<SaleResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateSale")
            .Produces<ApiResponseWithData<SaleResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteSale")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/cancel", CancelAsync).WithName("CancelSale")
            .Produces<ApiResponseWithData<SaleResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/items/{itemId:guid}/cancel", CancelItemAsync).WithName("CancelSaleItem")
            .Produces<ApiResponseWithData<SaleResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateAsync(CreateSaleCommand command, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Results.Created($"/api/sales/{result.Id}", new ApiResponseWithData<SaleResult>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = result
        });
    }

    private static async Task<IResult> ListAsync(
        ISender mediator,
        CancellationToken ct,
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null)
    {
        var result = await mediator.Send(new ListSalesQuery { Page = page, Size = size, Order = order }, ct);

        return Results.Ok(new PaginatedResponse<SaleResult>
        {
            Success = true,
            Data = result.Items,
            CurrentPage = result.Page,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

    private static async Task<IResult> GetAsync(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSaleQuery(id), ct);
        return Results.Ok(new ApiResponseWithData<SaleResult> { Success = true, Data = result });
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateSaleCommand command, ISender mediator, CancellationToken ct)
    {
        command.Id = id;
        var result = await mediator.Send(command, ct);
        return Results.Ok(new ApiResponseWithData<SaleResult>
        {
            Success = true,
            Message = "Sale updated successfully",
            Data = result
        });
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteSaleCommand(id), ct);
        return Results.Ok(new ApiResponse { Success = true, Message = "Sale deleted successfully" });
    }

    private static async Task<IResult> CancelAsync(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CancelSaleCommand(id), ct);
        return Results.Ok(new ApiResponseWithData<SaleResult>
        {
            Success = true,
            Message = "Sale cancelled successfully",
            Data = result
        });
    }

    private static async Task<IResult> CancelItemAsync(Guid id, Guid itemId, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new CancelSaleItemCommand(id, itemId), ct);
        return Results.Ok(new ApiResponseWithData<SaleResult>
        {
            Success = true,
            Message = "Sale item cancelled successfully",
            Data = result
        });
    }
}
