using EShop.Application.ShoppingCarts.Commands.CreateCart;
using EShop.Contracts.ShoppingCart;

namespace EShop.Api.ShoppingCarts.Endpoints;

public sealed class CreateShoppingCartEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/carts", async (ISender sender, HashSet<ProductLineItemRequest> items) =>
        {
            var command = new CreateShoppingCartCommand(items);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).RequireAuthorization();
    }
}
