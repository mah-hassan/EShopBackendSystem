namespace EShop.Domain.Categories;

public interface ICategoryBrandsRepository
{
    void AddCategoryBrand(Guid categoryId, Guid brandId);
    Task<bool> IsBrandExsists(Guid categoryId, Guid brandId);
}
