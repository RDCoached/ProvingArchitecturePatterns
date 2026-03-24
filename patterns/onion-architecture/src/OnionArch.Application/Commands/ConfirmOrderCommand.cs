using OnionArch.Application.Interfaces;
using OnionArch.Domain.Common;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Commands;

public sealed record ConfirmOrderCommand(Guid OrderId);

public sealed class ConfirmOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> HandleAsync(
        ConfirmOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.From(command.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
            return Result.Failure("Order not found");

        var result = order.Confirm();
        if (result.IsFailure)
            return result;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return Result.Success();
    }
}
