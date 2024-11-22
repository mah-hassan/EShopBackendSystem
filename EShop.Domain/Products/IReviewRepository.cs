using EShop.Domain.Abstractions;

namespace EShop.Domain.Products;

public interface IReviewRepository
    : IBaseRepository<Review>
{
    Task<IReadOnlyList<Review>> GetProductReviews(Guid productId, CancellationToken cancellationToken = default);
    Task<(float AverrageRate, int Count)> GetSummary(Guid productId, CancellationToken cancellationToken = default);
}