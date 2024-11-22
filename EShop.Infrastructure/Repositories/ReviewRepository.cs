using EShop.Domain.Products;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShop.Infrastructure.Repositories;

internal sealed class ReviewRepository(ApplicationDbContext dbContext)
    : BaseRepository<Review>(dbContext), IReviewRepository
{
    public async Task<IReadOnlyList<Review>> GetProductReviews(Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await dbContext.Reviews.AsNoTracking()
                    .Where(r => r.ProductId == productId)
                    .ToListAsync(cancellationToken);
        return reviews.AsReadOnly();
    }

    public async Task<(float AverrageRate, int Count)> GetSummary(Guid productId, CancellationToken cancellationToken = default)
    {
        var summary = await dbContext.Reviews
            .AsNoTracking()
            .Where (r => r.ProductId == productId)
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                AverrageRate = (float)Math.Min(g.Average(r => r.Rating), 5),
                Count = g.Count()
            }).FirstOrDefaultAsync(cancellationToken);
        return summary is null ? (default, 0) : (summary.AverrageRate, summary.Count);
    }
}