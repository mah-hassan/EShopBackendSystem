namespace EShop.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(AddCategoryRequest dto)
    : ICommand<Guid>;
