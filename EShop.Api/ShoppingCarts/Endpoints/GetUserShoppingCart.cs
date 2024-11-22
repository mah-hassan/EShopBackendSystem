
using EShop.Application.ShoppingCarts.Queries.GetUserShoppingCart;

namespace EShop.Api.ShoppingCarts.Endpoints;

public sealed class GetUserShoppingCart
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/carts/", async (ISender sender) =>
        {
            var query = new GetUserShoppingCartQuery();
            var result = await sender.Send(query);
            return result.ToResponse();
        }).RequireAuthorization();
    }
}
