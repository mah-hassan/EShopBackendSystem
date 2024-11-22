using EShop.Api.Filters;
using EShop.Application.Brands.Commands.UpdateBrand;
using EShop.Contracts.Brand;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Brands.Endpoints;

public sealed class UpdateBrandEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/brands/{brandId}", async (ISender sender, Guid brandId,[FromForm] BrandRequest request) =>
        {
            var command = new UpdateBrandCommand(brandId, request);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).AddEndpointFilter<ValidationFilter<BrandRequest>>().DisableAntiforgery();
    }
}
