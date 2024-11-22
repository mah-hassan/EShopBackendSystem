using EShop.Api.Filters;
using EShop.Application.Brands.Commands.AddBrand;
using EShop.Contracts.Brand;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Brands.Endpoints;

public class CreateBrandEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/brands", async (ISender sender, [FromForm] BrandRequest request) =>
        {
            var command = new CreateBrandCommand(request);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).AddEndpointFilter<ValidationFilter<BrandRequest>>()
            .DisableAntiforgery();
    }
}