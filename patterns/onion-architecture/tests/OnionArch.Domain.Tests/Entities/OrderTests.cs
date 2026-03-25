using FluentAssertions;
using OnionArch.Domain.Entities;
using OnionArch.Domain.Enums;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Domain.Tests.Entities;

public sealed class OrderTests
{
    [Fact]
    public void Create_ValidCustomerId_CreatesOrderInDraftStatus()
    {
        // Arrange
        var customerId = CustomerId.New();

        // Act
        var order = Order.Create(customerId);

        // Assert
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Draft);
        order.TotalAmount.Amount.Should().Be(0);
        order.TotalAmount.Currency.Should().Be("USD");
        order.Items.Should().BeEmpty();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithCustomCurrency_UsesSpecifiedCurrency()
    {
        // Arrange
        var customerId = CustomerId.New();

        // Act
        var order = Order.Create(customerId, "EUR");

        // Assert
        order.TotalAmount.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_NullCustomerId_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => Order.Create(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddItem_ValidItem_AddsItemAndRecalculatesTotal()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId = ProductId.New();
        var quantity = Quantity.Create(2);
        var unitPrice = Money.Create(50m, "USD");

        // Act
        var result = order.AddItem(productId, quantity, unitPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Amount.Should().Be(100m);
    }

    [Fact]
    public void AddItem_SameProductTwice_UpdatesQuantity()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId = ProductId.New();
        var quantity1 = Quantity.Create(2);
        var quantity2 = Quantity.Create(3);
        var unitPrice = Money.Create(50m, "USD");

        // Act
        order.AddItem(productId, quantity1, unitPrice);
        var result = order.AddItem(productId, quantity2, unitPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Value.Should().Be(5); // 2 + 3
        order.TotalAmount.Amount.Should().Be(250m); // 5 * 50
    }

    [Fact]
    public void AddItem_WhenOrderNotDraft_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId = ProductId.New();
        var quantity = Quantity.Create(2);
        var unitPrice = Money.Create(50m, "USD");

        order.AddItem(productId, quantity, unitPrice);
        order.Confirm();

        // Act
        var result = order.AddItem(ProductId.New(), quantity, unitPrice);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot modify order that is not in draft status");
    }

    [Fact]
    public void AddItem_WrongCurrency_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), "USD");
        var productId = ProductId.New();
        var quantity = Quantity.Create(2);
        var unitPrice = Money.Create(50m, "EUR");

        // Act
        var result = order.AddItem(productId, quantity, unitPrice);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("currency must be USD");
    }

    [Fact]
    public void RemoveItem_ExistingItem_RemovesItemAndRecalculatesTotal()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId1 = ProductId.New();
        var productId2 = ProductId.New();
        var quantity = Quantity.Create(2);
        var unitPrice = Money.Create(50m, "USD");

        order.AddItem(productId1, quantity, unitPrice);
        order.AddItem(productId2, quantity, unitPrice);

        // Act
        var result = order.RemoveItem(productId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.Items[0].ProductId.Should().Be(productId2);
        order.TotalAmount.Amount.Should().Be(100m);
    }

    [Fact]
    public void RemoveItem_NonExistentItem_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId = ProductId.New();

        // Act
        var result = order.RemoveItem(productId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Item not found in order");
    }

    [Fact]
    public void Confirm_ValidDraftOrder_SetsStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        var productId = ProductId.New();
        order.AddItem(productId, Quantity.Create(1), Money.Create(50m, "USD"));

        // Act
        var result = order.Confirm();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Confirm_EmptyOrder_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());

        // Act
        var result = order.Confirm();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot confirm empty order");
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();

        // Act
        var result = order.Confirm();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order can only be confirmed from draft status");
    }

    [Fact]
    public void MarkAsPaid_ConfirmedOrder_SetsStatusToPaid()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();

        // Act
        var result = order.MarkAsPaid();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_NotConfirmed_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());

        // Act
        var result = order.MarkAsPaid();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be confirmed before payment");
    }

    [Fact]
    public void Ship_PaidOrder_SetsStatusToShipped()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();
        order.MarkAsPaid();

        // Act
        var result = order.Ship();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_NotPaid_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();

        // Act
        var result = order.Ship();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Order must be paid before shipping");
    }

    [Fact]
    public void Deliver_ShippedOrder_SetsStatusToDelivered()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();
        order.MarkAsPaid();
        order.Ship();

        // Act
        var result = order.Deliver();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Cancel_DraftOrder_SetsStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShippedOrder_ReturnsFailure()
    {
        // Arrange
        var order = Order.Create(CustomerId.New());
        order.AddItem(ProductId.New(), Quantity.Create(1), Money.Create(50m, "USD"));
        order.Confirm();
        order.MarkAsPaid();
        order.Ship();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot cancel order");
    }
}
