using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Products;
using EShop.Contracts.Reviews;
using EShop.Contracts.Shared;
using EShop.Domain.Products;

namespace EShop.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(Guid? categoryId = null,
    Guid? brandId = null,
    string? orderBy = null,
    string? orderType = null,
    int? pageNumber = null,
    int? size = null)
    : IQuery<PaginatedResult<ProductResponse>>;

internal sealed class GetProductsQueryHandler(
    IProductRepository productRepository,
    IReviewRepository reviewRepository,
    Mapper mapper,
    ISupabaseService supabaseService)
        : IQueryHandler<GetProductsQuery, PaginatedResult<ProductResponse>>
{
    public async Task<Result<PaginatedResult<ProductResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetProductsSecification(request);
        var products = await productRepository.GetProductsWithSpecificationAsync(specification, cancellationToken);
        int count = await productRepository.CountAsync(specification.DisablePagenation());
        var items = new List<ProductResponse>();
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
            items.Add(productResponse);
        }

        return new PaginatedResult<ProductResponse>(items,
            request.pageNumber.HasValue ? (int)request.pageNumber : 0,
            request.size.HasValue ? (int)request.size : 0,
            count);

    }
}