using FluentAssertions;
using OnionArch.Domain.Entities;
using OnionArch.Domain.ValueObjects;
using OnionArch.Infrastructure.Repositories;

namespace OnionArch.Infrastructure.Tests.Repositories;

public sealed class OrderRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ValidOrder_SavesAndReturnsOrder()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var order = Order.Create(CustomerId.New(), "USD");

        // Act
        var result = await repository.AddAsync(order);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNull();
        result.Status.Should().Be(order.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var order = Order.Create(CustomerId.New(), "EUR");
        order.AddItem(ProductId.New(), Quantity.Create(2), Money.Create(50m, "EUR"));
        await repository.AddAsync(order);

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(order.CustomerId);
        result.TotalAmount.Amount.Should().Be(100m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentOrder_ReturnsNull()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var orderId = OrderId.New();

        // Act
        var result = await repository.GetByIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifiedOrder_SavesChanges()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var order = Order.Create(CustomerId.New(), "USD");
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        await repository.AddAsync(order);

        // Act
        order.Confirm();
        await repository.UpdateAsync(order);

        // Assert
        var retrieved = await repository.GetByIdAsync(order.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Status.Should().Be(order.Status);
        retrieved.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ReturnsCustomerOrders()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var customerId = CustomerId.New();

        var order1 = Order.Create(customerId, "USD");
        var order2 = Order.Create(customerId, "USD");
        var order3 = Order.Create(CustomerId.New(), "USD"); // Different customer

        await repository.AddAsync(order1);
        await repository.AddAsync(order2);
        await repository.AddAsync(order3);

        // Act
        var results = await repository.GetByCustomerIdAsync(customerId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(o => o.CustomerId.Should().Be(customerId));
    }
}
