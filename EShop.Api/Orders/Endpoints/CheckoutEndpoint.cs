
using EShop.Application.Orders.Commands.Checkout;

namespace EShop.Api.Orders.Endpoints;

public class CheckoutEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/{id}/checkout", async (ISender sender, Guid id) =>
        {
            var command = new StartOrderCheckoutSessionCommand(id);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
}
