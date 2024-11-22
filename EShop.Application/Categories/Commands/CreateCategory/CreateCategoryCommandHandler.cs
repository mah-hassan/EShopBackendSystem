using EShop.Domain.Categories;
using EShop.Domain.Shared.Errors;
using EShop.Domain.Products;
using EShop.Application.Abstractions.Mappers;
namespace EShop.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    Mapper mapper)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await categoryRepository.IsNameExsists(request.dto.Name))
        {
            return Result.Failure<Guid>(new Error("Category", "Category already exists", ErrorType.Conflict));
        }

        if(!request.dto.IsParentCategory && request.dto.ParentCategoryId == Guid.Empty)
        {
            return Result.Failure<Guid>(new Error("Category", "Parent category is required", ErrorType.BadRequest));
        }

        if(!request.dto.IsParentCategory && await categoryRepository.GetByIdAsync(request.dto.ParentCategoryId) is null)
        {
            return Result.Failure<Guid>(new Error("Category", "Parent category not found", ErrorType.NotFound));
        }

        var category = mapper.MapToCategory(request.dto);


        if (!category.IsParentCategory)
        {
            category.Variants = request.dto
                .Variants
                .Select(v => new Variant
                {
                    Name = v.Name,
                    Options = v.Values
                    .Select(val => new VariantOption 
                    { 
                        Value = val,
                    }).ToList()                    
                }).ToList();
        }

        categoryRepository.Add(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}
