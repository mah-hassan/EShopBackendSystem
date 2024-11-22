
using EShop.Application.Products.Commands.DeleteProduct;

namespace EShop.Api.Products.Endpoints;

public sealed class DeleteProductEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapDelete("/products/{id}", async (ISender sender, Guid id) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await sender.Send(command);
            return result.ToResponse(); 
        });
    }
}
