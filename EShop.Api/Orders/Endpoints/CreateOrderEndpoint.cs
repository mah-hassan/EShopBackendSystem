
using EShop.Application.Orders.Commands.CreateOrder;
using EShop.Contracts.Orders;

namespace EShop.Api.Orders.Endpoints;

public sealed class CreateOrderEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (ISender sender, OrderRequest request) =>
        {
            var command = new CreateOrderCommand(request);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).RequireAuthorization();
    }
}
