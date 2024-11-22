
using EShop.Application.ShoppingCarts.Commands.UpdateShoppingCartItem;
using EShop.Contracts.ShoppingCart;

namespace EShop.Api.ShoppingCarts.Endpoints;

public sealed class UpdateShoppingCartItemEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/carts/items/{id}", async (ISender sender,Guid id, ProductLineItemRequest request) =>
        {
            var command = new UpdateShoppingCartItemCommand(request, id);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
}