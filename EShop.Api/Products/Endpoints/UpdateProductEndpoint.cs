
using EShop.Application.Products.Commands.UpdateProduct;
using EShop.Contracts.Products;

namespace EShop.Api.Products.Endpoints;

public sealed class UpdateProductEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/{id}", async (ISender sender, Guid id, UpdateProductRequest product) =>
        {
            var command = new UpdateProductCommand(id, product);
            var result = await sender.Send(command);
            return result.ToResponse();
        });

    }
}