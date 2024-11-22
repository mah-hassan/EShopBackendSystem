using EShop.Application.Abstractions.Mappers;
using EShop.Domain.Categories;

namespace EShop.Application.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery()
    : ICachedQuery<List<CategoryResponse>>
{
    public string CachKey => $"categories-all";

    public TimeSpan? Period => null;
}

internal sealed class GetAllCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    Mapper mapper)
    : IQueryHandler<GetAllCategoriesQuery, List<CategoryResponse>>
{
    public async Task<Result<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        List<CategoryResponse> result = categories
            .Where(c => c.IsParentCategory)
            .Select(c => new CategoryResponse
            {
                Name = c.Name,
                Id = c.Id,
                SubCategories = categories.Where(sub => sub.ParentCategoryId == c.Id &&
                !sub.IsParentCategory)
                .Select(sub => new SubCategory
                {
                    Id = sub.Id,
                    Name = sub.Name,
                    ParentCategoryId = sub.ParentCategoryId,
                    Brands = sub.Brands.Select(b => mapper.MapToBrandResponse(b)).ToList(),
                })
                .ToList()

            })
        .ToList();
        return result;
    }
}
