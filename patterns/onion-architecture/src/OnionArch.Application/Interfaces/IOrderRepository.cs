using OnionArch.Domain.Entities;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default);
}
