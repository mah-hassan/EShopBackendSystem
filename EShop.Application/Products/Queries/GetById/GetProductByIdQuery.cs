using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Products;
using EShop.Contracts.Reviews;
using EShop.Contracts.Variants;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Products.Queries.GetById;

public sealed record GetProductByIdQuery(Guid id)
    : ICachedQuery<ProductDetails>
{
    public string CachKey => $"products-{id}";

    public TimeSpan? Period => TimeSpan.FromMinutes(10);
}

internal sealed class GetProductByIdQueryHandler
    (IProductRepository productRepository,
    ISupabaseService supabaseService,
    IReviewRepository reviewRepository,
    IProductAttribuatesRepository productAttribuatesRepository, 
    Mapper mapper)
    : IQueryHandler<GetProductByIdQuery, ProductDetails>
{
    public async Task<Result<ProductDetails>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.id);

        if(product is null)
        {
            return Result.Failure<ProductDetails>(new Error("Product", "Product not found", ErrorType.NotFound));
        }
       
        var productDetails = mapper.MapToProductDetails(product);

        productDetails.PrimaryImage =  supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);
    
        var attributes = await productAttribuatesRepository
            .GetProductAttributesAsync(product.Id);
        
        foreach (var variant in product.Variants)
        {
            productDetails.Variants.Add(
                    new VariantResponse
                    {
                        Id = variant.Key.Id,
                        Name = variant.Key.Name,
                        Options = variant
                        .Select(op => new Option
                        {
                            Id = op.Id,
                            Value = op.Value
                        }).ToList(),
                    });                  
        }

        foreach (var image in product.Images)
        {
            productDetails.Images.Add(supabaseService.GetPublicUrl(SupabaseBackets.Products, image));
        }

        var reviewSummary = await reviewRepository.GetSummary(product.Id, cancellationToken);
        productDetails.ReviewSummary = new ReviewSummary
        {
            AverageRating = reviewSummary.AverrageRate,
            Count = reviewSummary.Count,
        };
        return productDetails;
    }
}