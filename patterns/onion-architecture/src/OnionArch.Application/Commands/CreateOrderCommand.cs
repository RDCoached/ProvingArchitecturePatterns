using OnionArch.Application.DTOs;
using OnionArch.Application.Interfaces;
using OnionArch.Domain.Common;
using OnionArch.Domain.Entities;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Commands;

public sealed record CreateOrderCommand(Guid CustomerId, string Currency = "USD");

public sealed class CreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.From(command.CustomerId);
        var order = Order.Create(customerId, command.Currency);

        await _orderRepository.AddAsync(order, cancellationToken);

        var dto = MapToDto(order);
        return Result.Success(dto);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id.Value,
            order.CustomerId.Value,
            order.Status,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.CreatedAt,
            order.ConfirmedAt,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId.Value,
                i.Quantity.Value,
                i.UnitPrice.Amount,
                i.TotalPrice.Amount,
                i.UnitPrice.Currency
            )).ToList()
        );
    }
}
