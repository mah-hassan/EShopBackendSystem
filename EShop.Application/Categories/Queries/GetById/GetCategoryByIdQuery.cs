
using EShop.Application.Abstractions.Mappers;
using EShop.Domain.Categories;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Categories.Queries.GetById;

public sealed record GetCategoryByIdQuery(Guid id)
    : ICachedQuery<CategoryDetails>
{
    public string CachKey => $"categories-{id}";

    public TimeSpan? Period => null;
}

internal sealed class GetCategoryByIdQueryHandler
    (ICategoryRepository categoryRepository,
    Mapper mapper)
    : IQueryHandler<GetCategoryByIdQuery, CategoryDetails>
{
    public async Task<Result<CategoryDetails>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.id);

        if (category is null)
        {
            return Result.Failure<CategoryDetails>(new Error("Category", "Category not found", ErrorType.NotFound));
        }

        var categoryDetails = mapper.MapToCategoryDetails(category);
           
        return categoryDetails;
    }
}