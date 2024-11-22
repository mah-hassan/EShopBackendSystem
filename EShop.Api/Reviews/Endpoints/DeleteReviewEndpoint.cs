
using EShop.Application.Reviews.Commands.DeleteReview;

namespace EShop.Api.Reviews.Endpoints
{
    public sealed class DeleteReviewEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/reviews/{id}", async (ISender sender, Guid id) =>
            {
                var command = new DeleteReviewCommand(id);
                var result = await sender.Send(command);
                return result.ToResponse();
            }).RequireAuthorization()
            .WithTags("Reviews");
        }
    }
}
