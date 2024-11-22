
using EShop.Application.Orders.Queries.GetById;

namespace EShop.Api.Orders.Endpoints;

public sealed class GetOrderByIdEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{id}", async (ISender sender, Guid id) =>
        {
            var query = new GetOrderByIdQuery(id);
            var result = await sender.Send(query);
            return result.ToResponse();
        });

    }

}