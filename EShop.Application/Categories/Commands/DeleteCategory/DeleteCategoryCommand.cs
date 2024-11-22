
using EShop.Domain.Categories;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid id)
    : ICommand;

internal sealed class DeleteCategoryCommandHandler
    : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.id);

        if (category is null)
        {
            return Result.Failure(new Error("Category", "Category not found", ErrorType.NotFound));
        }

        _categoryRepository.Delete(category);
        if (category.IsParentCategory)
        {
            // TODO: remove sub categories
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
