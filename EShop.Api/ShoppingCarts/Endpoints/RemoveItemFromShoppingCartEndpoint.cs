
using EShop.Application.ShoppingCarts.Commands.RemoveItemFromCart;

namespace EShop.Api.ShoppingCarts.Endpoints;

public sealed class RemoveItemFromShoppingCartEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapDelete("/carts/items/{itemId}", async (ISender sender, Guid itemId) =>
        {
            var command = new RemoveItemFromCartCommand(itemId);

            var result = await sender.Send(command);

            return result.ToResponse();
        });

    }
}