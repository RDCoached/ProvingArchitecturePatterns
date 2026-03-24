using OnionArch.Application.DTOs;
using OnionArch.Application.Interfaces;
using OnionArch.Domain.Common;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId);

public sealed class GetOrderByIdQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.From(query.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDto>("Order not found");

        var dto = new OrderDto(
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

        return Result.Success(dto);
    }
}
