
using EShop.Application.Reviews.Queries.GetProductReviews;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Reviews.Endpoints
{
    public class GetProductReviewsEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/reviews", async (ISender sender,[FromQuery] Guid productId) =>
            {
                var query = new GetProductReviewsQuery(productId);
                var result = await sender.Send(query);
                return result.ToResponse();
            }).WithTags("Reviews");
        }
    }
}
