using OnionArch.Domain.Enums;

namespace OnionArch.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    List<OrderItemDto> Items
);

public sealed record OrderItemDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency
);
