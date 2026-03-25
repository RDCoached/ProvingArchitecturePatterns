using FluentAssertions;
using NSubstitute;
using OnionArch.Application.Commands;
using OnionArch.Application.Interfaces;
using OnionArch.Domain.Entities;
using OnionArch.Domain.Enums;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Tests.Commands;

public sealed class ConfirmOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new ConfirmOrderCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_ConfirmsOrderAndReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ConfirmOrderCommand(orderId);

        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));

        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);

        await _repository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ConfirmOrderCommand(orderId);

        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order not found");

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EmptyOrder_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ConfirmOrderCommand(orderId);

        var order = Order.Create(CustomerId.New());
        // Don't add any items

        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot confirm empty order");

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AlreadyConfirmed_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ConfirmOrderCommand(orderId);

        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm(); // Already confirmed

        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("draft status");
    }
}
