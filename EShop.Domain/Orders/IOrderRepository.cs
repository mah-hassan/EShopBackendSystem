using EShop.Domain.Abstractions;

namespace EShop.Domain.Orders;

public interface IOrderRepository
    : IBaseRepository<Order>
{
    Task<IReadOnlyList<Order>> GetAllByCustomerEmailAsync(string cutomerEmail);
    Task RemoveProductFromOrdersAsync(Guid productId);
}