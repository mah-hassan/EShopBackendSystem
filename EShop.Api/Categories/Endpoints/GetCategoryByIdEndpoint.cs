
using EShop.Application.Categories.Queries.GetById;

namespace EShop.Api.Categories.Endpoints;

public sealed class GetCategoryByIdEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/{id}", async (ISender sender, Guid id) =>
        {
            var query = new GetCategoryByIdQuery(id);
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}
