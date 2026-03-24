using OnionArch.Application.DTOs;
using OnionArch.Application.Interfaces;
using OnionArch.Domain.Common;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Queries;

public sealed record GetOrdersByCustomerQuery(Guid CustomerId);

public sealed class GetOrdersByCustomerQueryHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> HandleAsync(
        GetOrdersByCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        var customerId = CustomerId.From(query.CustomerId);
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        var dtos = orders.Select(order => new OrderDto(
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
        )).ToList();

        return Result.Success<IReadOnlyList<OrderDto>>(dtos);
    }
}
