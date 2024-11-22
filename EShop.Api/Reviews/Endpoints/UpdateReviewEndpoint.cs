using EShop.Api.Filters;
using EShop.Application.Reviews.Commands.UpdateReview;
using EShop.Contracts.Reviews;

namespace EShop.Api.Reviews.Endpoints
{
    public sealed class UpdateReviewEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/reviews/{id}", async (ISender sender, Guid id, ReviewRequest request) =>
            {
                var command = new UpdateReviewCommand(id, request);
                var result = await sender.Send(command);
                return result.ToResponse();
            }).RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<ReviewRequest>>()
            .WithTags("Reviews");
        }
    }
}