using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Products;
using EShop.Contracts.Reviews;
using EShop.Domain.Products;

namespace EShop.Application.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery
    : ICachedQuery<List<ProductResponse>>
{
    public string CachKey => "products-all";

    public TimeSpan? Period => TimeSpan.FromMinutes(30);
}

internal sealed class GetAllProductsQueryHandler(IProductRepository productRepository, ISupabaseService supabaseService, Mapper mapper, IReviewRepository reviewRepository)
        : IQueryHandler<GetAllProductsQuery, List<ProductResponse>>
{

    public async Task<Result<List<ProductResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        List<ProductResponse> result = new();

        foreach (var product in products)
        {
            var productResponse = mapper.MapToProductResponse(product);

            productResponse.PrimaryImage = supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);
            var reviewSummary = await reviewRepository.GetSummary(product.Id, cancellationToken);
            productResponse.ReviewSummary = new ReviewSummary
            {
                AverageRating = reviewSummary.AverrageRate,
                Count = reviewSummary.Count,
            };
            result.Add(productResponse);
        }
        
        return result;
    }
}