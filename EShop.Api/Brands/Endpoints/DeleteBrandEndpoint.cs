
using EShop.Application.Brands.Commands.DeleteBrand;

namespace EShop.Api.Brands.Endpoints;

public class DeleteBrandEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/brands/{brandId}", async (ISender sender, Guid brandId) =>
        {
            var command = new DeleteBrandCommand(brandId);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
}
