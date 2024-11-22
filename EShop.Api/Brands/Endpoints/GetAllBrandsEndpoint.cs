
using EShop.Application.Brands.Queries.GetAllBrands;

namespace EShop.Api.Brands.Endpoints;

public sealed class GetAllBrandsEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/brands", async (ISender sender) =>
        {
            var query = new GetAllBrandsQuery();
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}
