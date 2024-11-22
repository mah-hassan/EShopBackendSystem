
using EShop.Application.Orders.Queries.GetAll;

namespace EShop.Api.Orders.Endpoints
{
    public class GetAllUserOrdersEndpoint
        : IEndpoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/orders/", async (ISender sender) =>
            {
                var query = new GetAllOrdersForAUserQuery();
                var result = await sender.Send(query);
                return result.ToResponse();
            }).RequireAuthorization();
        }
    }
}
