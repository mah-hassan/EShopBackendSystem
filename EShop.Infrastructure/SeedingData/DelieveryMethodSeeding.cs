using EShop.Domain.Orders;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.SeedingData;

internal class DelieveryMethodSeeding
{
   

    public async static Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (!await dbContext.Set<DeliveryMethod>().AnyAsync())
        {
            var deliveryMethods = new List<DeliveryMethod>()
            {
                new ()
                {
                    Name = "Standard Delivery",
                    Description = "Delivery within 3-5 business days",
                    DeliveryCost = 50.00m
                },
                new ()
                {
                    Name = "Express Delivery",
                    Description = "Delivery within 1-2 business days",
                    DeliveryCost = 100.00m
                },
                new ()
                {
                    Name = "Same Day Delivery",
                    Description = "Delivery on the same day of order",
                    DeliveryCost = 150.00m
                }
            };
            dbContext.Set<DeliveryMethod>().AddRange(deliveryMethods);
            await dbContext.SaveChangesAsync();
        }
    }
}