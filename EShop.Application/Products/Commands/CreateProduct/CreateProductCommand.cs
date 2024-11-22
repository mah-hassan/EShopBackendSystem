using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Products;
using EShop.Domain.Brands;
using EShop.Domain.Categories;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(ProductRequest ProductRequest)
    : ICommand<Guid>;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ISupabaseService supabaseService,
    IProductAttribuatesRepository productAttribuatesRepository,
    IBrandRepository brandRepository,
    IEventBus eventBus,
    ICategoryRepository categoryRepository,
    ICategoryBrandsRepository categoryBrandsRepository,
    Mapper mapper)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await productRepository.IsSkuUnique(request.ProductRequest.Sku))
        {
            return Result.Failure<Guid>(new Error("Sku", "Product Sku already exists", ErrorType.Conflict));
        }
        if(await brandRepository.GetByIdAsync(request.ProductRequest.BrandId) is null)
        {
            return Result.Failure<Guid>(new Error("Brand", "Brand not found", ErrorType.NotFound));
        }

        var category = await categoryRepository.GetByIdAsync(request.ProductRequest.CategoryId);

        if (category is null)
        {
            return Result.Failure<Guid>(new Error("Category", "Category not found", ErrorType.NotFound));
        }

        var product = mapper.MapToProduct(request.ProductRequest);

        product.PrimaryImage = await supabaseService
            .UploadAsync(request.ProductRequest.PrimaryImage!,
            SupabaseBackets.Products,
            $"product-{product.Id}{Path.GetExtension(request.ProductRequest.PrimaryImage?.FileName)}");

        foreach (var Attribuate in request.ProductRequest.Attribuates)
        {
            productAttribuatesRepository
                .AddProductAttribuate(
                product.Id,
                Attribuate);           
        }

        if(request.ProductRequest.Images.Any())
        {
            foreach (var Image in request.ProductRequest.Images)
            {
                var supabasePath = await supabaseService.UploadAsync(Image,
                    SupabaseBackets.Products,
                    $"product-{product.Id}-{Path.GetRandomFileName()}{Path.GetExtension(Image.FileName)}");

                product.Images.Add(supabasePath);
            }
        }
        productRepository.Add(product);

        if (!await categoryBrandsRepository.IsBrandExsists(product.CategoryId, product.BrandId))
        {
            categoryBrandsRepository.AddCategoryBrand(product.CategoryId, product.BrandId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(new ProductCreatedEvent
        {
            Id = product.Id,
            ProductName = product.Name,
            ProductDescription = product.Description,
            Price = product.Price,
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            PrimaryImage = product.PrimaryImage,
            SKU = product.Sku,
        });

        return product.Id;
    }
}