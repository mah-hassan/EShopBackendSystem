
using EShop.Application.Products.Queries.GetById;

namespace EShop.Api.Products.Endpoints;

public sealed class GetProductByIdEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (ISender sender, Guid id) =>
        {
            var query = new GetProductByIdQuery(id);
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}
