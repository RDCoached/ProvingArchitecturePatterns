using OnionArch.Domain.Common;
using OnionArch.Domain.Enums;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    private Order(OrderId id, CustomerId customerId, string currency)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        TotalAmount = Money.Zero(currency);
        CreatedAt = DateTime.UtcNow;
    }

    public static Order Create(CustomerId customerId, string currency = "USD")
    {
        if (customerId is null)
            throw new ArgumentNullException(nameof(customerId));

        return new Order(OrderId.New(), customerId, currency);
    }

    public Result AddItem(ProductId productId, Quantity quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Cannot modify order that is not in draft status");

        if (productId is null)
            return Result.Failure("Product ID is required");

        if (quantity is null)
            return Result.Failure("Quantity is required");

        if (unitPrice is null)
            return Result.Failure("Unit price is required");

        if (unitPrice.Currency != TotalAmount.Currency)
            return Result.Failure($"Unit price currency must be {TotalAmount.Currency}");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity.Add(quantity);
            existingItem.UpdateQuantity(newQuantity);
        }
        else
        {
            var item = OrderItem.Create(productId, quantity, unitPrice);
            _items.Add(item);
        }

        RecalculateTotal();
        return Result.Success();
    }

    public Result RemoveItem(ProductId productId)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Cannot modify order that is not in draft status");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure("Item not found in order");

        _items.Remove(item);
        RecalculateTotal();
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Order can only be confirmed from draft status");

        if (!_items.Any())
            return Result.Failure("Cannot confirm empty order");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            return Result.Failure($"Cannot cancel order with status {Status}");

        Status = OrderStatus.Cancelled;
        return Result.Success();
    }

    public Result MarkAsPaid()
    {
        if (Status != OrderStatus.Confirmed)
            return Result.Failure("Order must be confirmed before payment");

        Status = OrderStatus.Paid;
        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != OrderStatus.Paid)
            return Result.Failure("Order must be paid before shipping");

        Status = OrderStatus.Shipped;
        return Result.Success();
    }

    public Result Deliver()
    {
        if (Status != OrderStatus.Shipped)
            return Result.Failure("Order must be shipped before delivery");

        Status = OrderStatus.Delivered;
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        var total = Money.Zero(TotalAmount.Currency);
        foreach (var item in _items)
        {
            total = total.Add(item.TotalPrice);
        }
        TotalAmount = total;
    }
}
