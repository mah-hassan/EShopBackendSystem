
using EShop.Api.Filters;
using EShop.Application.Reviews.Commands.AddReview;
using EShop.Contracts.Reviews;

namespace EShop.Api.Reviews.Endpoints;

public sealed class AddReviewEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reviews", async (ISender sender, ReviewRequest paylood) =>
        {
            var command = new AddReviewCommand(paylood);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).RequireAuthorization()
        .AddEndpointFilter<ValidationFilter<ReviewRequest>>()
        .WithTags("Reviews");
    }
}