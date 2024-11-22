using EShop.Application.Abstractions.Mappers;
using EShop.Contracts.Products;
using EShop.Domain.Products;

namespace EShop.Application.Products.Queries.Search;

public sealed record ProductsSearchQuery(string term)
    : ICachedQuery<List<ProductResponse>>
{
    public string CachKey => $"product-src-{term}";

    public TimeSpan? Period => default;
}

internal sealed class ProductsSearchQueryHandler(
    IElasticSearchService elasticSearchService,
    IReviewRepository reviewRepository,
    Mapper mapper) : IQueryHandler<ProductsSearchQuery, List<ProductResponse>>
{
    public async Task<Result<List<ProductResponse>>> Handle(ProductsSearchQuery request, CancellationToken cancellationToken)
    {
        var searchResult = await elasticSearchService.SearchAsync<ElasticSearchProduct>(request.term);
        var result = new List<ProductResponse>();
        foreach (var product in searchResult)
        {
            ProductResponse productResponse = mapper.MapToProductResponse(product);
            var reviewSummary = await reviewRepository.GetSummary(product.Id, cancellationToken);
            productResponse.ReviewSummary = new()
            {
                AverageRating = reviewSummary.AverrageRate,
                Count = reviewSummary.Count,
            };
            result.Add(productResponse);
        }
        return result;
    }
}