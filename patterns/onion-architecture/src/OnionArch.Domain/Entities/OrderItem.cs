using OnionArch.Domain.ValueObjects;

namespace OnionArch.Domain.Entities;

public sealed class OrderItem
{
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice { get; private set; }

    private OrderItem(ProductId productId, Quantity quantity, Money unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.Multiply(quantity.Value);
    }

    public static OrderItem Create(ProductId productId, Quantity quantity, Money unitPrice)
    {
        if (productId is null)
            throw new ArgumentNullException(nameof(productId));

        if (quantity is null)
            throw new ArgumentNullException(nameof(quantity));

        if (unitPrice is null)
            throw new ArgumentNullException(nameof(unitPrice));

        return new OrderItem(productId, quantity, unitPrice);
    }

    public void UpdateQuantity(Quantity newQuantity)
    {
        if (newQuantity is null)
            throw new ArgumentNullException(nameof(newQuantity));

        Quantity = newQuantity;
        TotalPrice = UnitPrice.Multiply(newQuantity.Value);
    }
}
