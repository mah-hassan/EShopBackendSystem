
using EShop.Application.ShoppingCarts.Commands.AddItemsToCart;
using EShop.Contracts.ShoppingCart;

namespace EShop.Api.ShoppingCarts.Endpoints
{
    public sealed class AddItemToCartEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/carts/items", async (ISender sender, ProductLineItemRequest request) =>
            {
                var command = new AddItemToCartCommand(request);
                var result = await sender.Send(command);
                return result.ToResponse();
            }).RequireAuthorization();
        }
    }
}
