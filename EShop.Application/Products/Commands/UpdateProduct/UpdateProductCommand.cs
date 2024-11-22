using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Products;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, UpdateProductRequest UpdatedProduct)
    : ICommand;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IProductAttribuatesRepository productAttribuatesRepository,
    ISupabaseService supabaseService,
    IEventBus eventBus,
    IUnitOfWork unitOfWork,
    Mapper mapper)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);
        if (product is null)
        {
            return Result.Failure<ProductDetails>(new Error("Product", "Product not found", ErrorType.NotFound));
        }
        product.Name = request.UpdatedProduct.Name;
        product.Description = request.UpdatedProduct.Description;
        product.Price = mapper.MapToMoney(request.UpdatedProduct.Price);

        // PrimaryImage is not null here because it passed through validation filter
        var deleteTask = supabaseService.DeleteFileAsync(SupabaseBackets.Products, product.PrimaryImage);
        var uploadTask = supabaseService.UploadAsync(request.UpdatedProduct.PrimaryImage!, SupabaseBackets.Products,
            $"product-{product.Id}{Path.GetExtension(request.UpdatedProduct.PrimaryImage!.FileName)}");
        await Task.WhenAll(deleteTask, uploadTask);

        product.PrimaryImage = uploadTask.Result;

        var attribuates = await productAttribuatesRepository.GetProductAttributesAsync(product.Id);

        foreach (var Attribuate in request.UpdatedProduct.Attribuates)
        {
            if (attribuates.Any(a => a.VariantOptionId == Attribuate))
            {
                continue;
            }
            productAttribuatesRepository.AddProductAttribuate(product.Id, Attribuate);
        }
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);


        await eventBus.PublishAsync(new ProductUpdatedEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductDescription = product.Description,
            Price = product.Price,
            PrimaryImage = product.PrimaryImage,
            SKU = product.Sku,
            ProductCategoryId = product.CategoryId,
            ProductBrandId = product.BrandId,
        });


        return Result.Success();
    }
}