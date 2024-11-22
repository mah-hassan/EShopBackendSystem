using EShop.Domain.Products;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class ProductAttribuatesRepository(ApplicationDbContext dbContext)
    : IProductAttribuatesRepository
{

    public void AddProductAttribuate(Guid productId, Guid optionId)
    {
        dbContext.ProductAttributes
            .Add(new()
            {
                VariantOptionId = optionId,
                ProductId = productId,
            });
    }

    public async Task<List<ProductAttribuates>> GetProductAttributesAsync(Guid productId)
        => await dbContext.ProductAttributes.Where(pa => pa.ProductId == productId).ToListAsync();



    public Task RemoveProductAttribuatesAsync(Guid productId)
    {
        return dbContext
            .ProductAttributes
            .Where(pa => pa.ProductId == productId)
            .ExecuteDeleteAsync();
    }
}