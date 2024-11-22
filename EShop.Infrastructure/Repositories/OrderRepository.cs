using EShop.Domain.Orders;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class OrderRepository(ApplicationDbContext dbContext)
    : BaseRepository<Order>(dbContext), IOrderRepository
{
    public async Task<IReadOnlyList<Order>> GetAllByCustomerEmailAsync(string cutomerEmail)
    {
        return await dbContext.Orders.AsNoTracking()
            .Where(o => o.CustomerEmail == cutomerEmail)
            .ToListAsync();
    }

    public Task RemoveProductFromOrdersAsync(Guid productId)
    {
        return dbContext.Set<OrderItem>()
            .Where(oi => oi.ProductId == productId)
            .ExecuteDeleteAsync();
    }
}