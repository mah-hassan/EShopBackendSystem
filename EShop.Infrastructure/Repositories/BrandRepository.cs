using EShop.Domain.Brands;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class BrandRepository
    : BaseRepository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context) : base(context) { }


    public Task<Brand?> GetByIdIncludingProductsAsync(Guid id)
    {
        return dbContext
            .Brands
            .Where(b => b.Id == id)
            .Include(b => b.Products)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsNameExists(string name) 
        => await dbContext.Brands.AnyAsync(b => b.Name.ToLower() == name.ToLower());
}