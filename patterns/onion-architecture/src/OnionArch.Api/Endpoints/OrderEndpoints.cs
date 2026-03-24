using Microsoft.AspNetCore.Mvc;
using OnionArch.Application.Commands;
using OnionArch.Application.Queries;

namespace OnionArch.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // POST /api/orders - Create a new order
        group.MapPost("", async (
            [FromBody] CreateOrderRequest request,
            [FromServices] CreateOrderCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new CreateOrderCommand(request.CustomerId, request.Currency);
            var result = await handler.HandleAsync(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/orders/{result.Value!.Id}", result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order");

        // POST /api/orders/{id}/items - Add item to order
        group.MapPost("{id}/items", async (
            Guid id,
            [FromBody] AddOrderItemRequest request,
            [FromServices] AddOrderItemCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new AddOrderItemCommand(id, request.ProductId, request.Quantity, request.UnitPrice);
            var result = await handler.HandleAsync(command, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("AddOrderItem")
        .WithSummary("Add an item to an existing order");

        // POST /api/orders/{id}/confirm - Confirm order
        group.MapPost("{id}/confirm", async (
            Guid id,
            [FromServices] ConfirmOrderCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new ConfirmOrderCommand(id);
            var result = await handler.HandleAsync(command, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        .WithName("ConfirmOrder")
        .WithSummary("Confirm an order");

        // GET /api/orders/{id} - Get order by ID
        group.MapGet("{id}", async (
            Guid id,
            [FromServices] GetOrderByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetOrderByIdQuery(id);
            var result = await handler.HandleAsync(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetOrderById")
        .WithSummary("Get an order by ID");

        // GET /api/orders/customer/{customerId} - Get customer orders
        group.MapGet("customer/{customerId}", async (
            Guid customerId,
            [FromServices] GetOrdersByCustomerQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetOrdersByCustomerQuery(customerId);
            var result = await handler.HandleAsync(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        })
        .WithName("GetOrdersByCustomer")
        .WithSummary("Get all orders for a customer");
    }
}

// Request models
public sealed record CreateOrderRequest(Guid CustomerId, string Currency = "USD");
public sealed record AddOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice);
