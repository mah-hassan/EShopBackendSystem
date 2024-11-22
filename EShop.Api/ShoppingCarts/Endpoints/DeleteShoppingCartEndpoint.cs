
using EShop.Application.ShoppingCarts.Commands.DeleteShoppingCart;

namespace EShop.Api.ShoppingCarts.Endpoints;

public class DeleteShoppingCartEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapDelete("/carts", async (ISender sender) =>
        {
            var command = new DeleteShoppingCartCommand();

            var result = await sender.Send(command);

            return result.ToResponse(); 
        }).RequireAuthorization();
    }
}