using EShop.Domain.Categories;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class CategoryBrandsRepository(ApplicationDbContext dbContext)
    : ICategoryBrandsRepository
{
    public void AddCategoryBrand(Guid categoryId, Guid brandId)
    {
        dbContext.Set<CategoryBrands>().Add(new CategoryBrands() { BrandId = brandId, CategoryId = categoryId});
    }

    public async Task<bool> IsBrandExsists(Guid categoryId, Guid brandId)
    {
        return await dbContext.Set<CategoryBrands>().AnyAsync(cb => cb.CategoryId == categoryId && cb.BrandId == brandId);
    }
}