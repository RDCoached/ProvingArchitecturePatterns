using FluentAssertions;
using NSubstitute;
using OnionArch.Application.Commands;
using OnionArch.Application.Interfaces;
using OnionArch.Domain.Entities;
using OnionArch.Domain.Enums;

namespace OnionArch.Application.Tests.Commands;

public sealed class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _handler = new CreateOrderCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesOrderAndReturnsDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "USD");

        _repository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Order>()));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CustomerId.Should().Be(customerId);
        result.Value.Status.Should().Be(OrderStatus.Draft);
        result.Value.Currency.Should().Be("USD");
        result.Value.TotalAmount.Should().Be(0);
        result.Value.Items.Should().BeEmpty();

        await _repository.Received(1).AddAsync(
            Arg.Is<Order>(o => o.CustomerId.Value == customerId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithEuroCurrency_CreatesOrderInEuro()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var command = new CreateOrderCommand(customerId, "EUR");

        _repository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Order>()));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationToken_ToRepository()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid());
        var cts = new CancellationTokenSource();

        _repository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Order>()));

        // Act
        await _handler.HandleAsync(command, cts.Token);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Any<Order>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));
    }
}
