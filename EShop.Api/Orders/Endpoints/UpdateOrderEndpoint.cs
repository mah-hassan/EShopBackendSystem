using EShop.Application.Orders.Commands.UpdateOrder;
using EShop.Contracts.Orders;

namespace EShop.Api.Orders.Endpoints
{
    public class UpdateOrderEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPut("/orders/{id}", async (ISender sender, Guid id, UpdateOrderRequest request) =>
            {
                var command = new UpdateOrderCommand(id, request);  
                var result = await sender.Send(command);
                return result.ToResponse();
            });
        }
    }
}
