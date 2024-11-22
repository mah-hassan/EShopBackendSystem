
using EShop.Application.Orders.Commands.CancelOrder;

namespace EShop.Api.Orders.Endpoints;

public sealed class CancelOrderEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/orders/{id}/cancel", async (ISender sender, Guid id) =>
        {
            var command = new CancelOrderCommand(id);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
}
