using EShop.Application.Abstractions.Mappers;
using EShop.Contracts.Reviews;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Reviews.Queries.GetProductReviews;

public sealed record GetProductReviewsQuery(Guid ProductId)
    : ICachedQuery<List<ReviewResponse>>
{
    public string CachKey => $"pro-{ProductId}-reviews";

    public TimeSpan? Period => default;
}

internal sealed class GetProductReviewsQueryHandler(
    IProductRepository productRepository,
    IReviewRepository reviewRepository,
    Mapper mapper) 
    : IQueryHandler<GetProductReviewsQuery, List<ReviewResponse>>
{
    public async Task<Result<List<ReviewResponse>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId);
        if (product is null)
        {
            return Result.Failure<List<ReviewResponse>>(new Error("Product", "Product not found", ErrorType.NotFound));
        }
        var reviews = await reviewRepository.GetProductReviews(request.ProductId);
        List<ReviewResponse> response = new List<ReviewResponse>();
        foreach (var review in reviews)
        {
            ReviewResponse reviewResponse = mapper.MapToReviewResponse(review);

            // TODO: add user information to the response
            response.Add(reviewResponse);
        }
        return response;
    }
}