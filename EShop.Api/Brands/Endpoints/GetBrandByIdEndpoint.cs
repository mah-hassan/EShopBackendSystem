using EShop.Application.Brands.Queries.GetBrandById;

namespace EShop.Api.Brands.Endpoints;

public class GetBrandByIdEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/brands/{brandId}", async (ISender sender, Guid brandId) =>
        {
            var query = new GetBrandByIdQuery(brandId);
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}
