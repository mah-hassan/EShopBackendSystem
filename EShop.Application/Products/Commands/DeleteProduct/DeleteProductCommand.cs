
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id)
    : ICommand;

internal sealed class DeleteProductCommandHandler(
    IProductRepository productRepository, 
    IEventBus eventBus,
    IUnitOfWork unitOfWork)
        : ICommandHandler<DeleteProductCommand>
{

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);

        if (product is null)
        {
            return Result.Failure(new Error("Product", "Product not found", ErrorType.NotFound));
        }
   
        productRepository.Delete(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventBus.PublishAsync(new ProductDeletedEvent
        {
            ProductBrandId = product.BrandId,
            ProductName = product.Name,
            ProductCategoryId = product.CategoryId,
            ProductId = product.Id,
            PrimaryImage = product.PrimaryImage,
            Images = product.Images
        });
        return Result.Success();
    }
}