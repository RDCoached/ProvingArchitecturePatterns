using OnionArch.Application.Interfaces;
using OnionArch.Domain.Common;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Commands;

public sealed record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);

public sealed class AddOrderItemCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public AddOrderItemCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> HandleAsync(
        AddOrderItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.From(command.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result.Failure("Order not found");

        var productId = ProductId.From(command.ProductId);
        var quantity = Quantity.Create(command.Quantity);
        var unitPrice = Money.Create(command.UnitPrice, order.TotalAmount.Currency);

        var result = order.AddItem(productId, quantity, unitPrice);
        if (result.IsFailure)
            return result;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return Result.Success();
    }
}
