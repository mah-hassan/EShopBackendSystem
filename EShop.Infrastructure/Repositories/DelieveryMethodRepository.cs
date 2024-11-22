using EShop.Domain.Orders;
using EShop.Infrastructure.Data;
namespace EShop.Infrastructure.Repositories;

internal sealed class DeliveryMethodRepository(ApplicationDbContext dbContext)
    : BaseRepository<DeliveryMethod>(dbContext) , IDeliveryMethodRepository
{
}