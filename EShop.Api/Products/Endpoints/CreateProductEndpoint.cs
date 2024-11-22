using EShop.Api.Filters;
using EShop.Application.Products.Commands.CreateProduct;
using EShop.Contracts.Products;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Products.Endpoints;

public sealed class CreateProductEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (ISender sender, [FromForm] ProductRequest request) =>
        {
            var command = new CreateProductCommand(request);
            var result = await sender.Send(command);
            return result.ToResponse();
        }).AddEndpointFilter<ValidationFilter<ProductRequest>>()
            .DisableAntiforgery();
    }
}
