using EShop.Domain.Abstractions.Specifications;
using EShop.Domain.Products;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class ProductRepository(ApplicationDbContext dbContext)
    : BaseRepository<Product>(dbContext), IProductRepository
{
    public Task<int> CountAsync(Specification<Product> spc) => ApplaySpecifications(spc)
            .CountAsync();

    public override async Task<Product?> GetByIdAsync(Guid id)
        => await dbContext.Products
        .Include(p => p.VariantOptions)
        .ThenInclude(option => option.Variant)
        .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetProductsWithSpecificationAsync(Specification<Product> specification, CancellationToken cancellationToken)
        => await ApplaySpecifications(specification)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsSkuUnique(string sku)
        => !await dbContext.Products.AnyAsync(p => p.Sku == sku);
}