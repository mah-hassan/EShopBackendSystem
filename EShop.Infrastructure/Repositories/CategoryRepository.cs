using EShop.Domain.Categories;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class CategoryRepository
    : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<bool> IsNameExsists(string name)
        => await dbContext.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());

    public override async Task<Category?> GetByIdAsync(Guid id)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is not null && !category.IsParentCategory)
        {

            await dbContext
                .Entry(category)
                .Collection(c => c.Variants)
                .LoadAsync();

            await dbContext
            .Entry(category)
            .Collection(c => c.Brands)
            .LoadAsync();

        }
        return category;
    }

}