using EShop.Api.Authentication;
using EShop.Application.Products.Queries.GetAllProducts;

namespace EShop.Api.Products.Endpoints;

public sealed class GetAllProductsEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender) =>
        {
            var query = new GetAllProductsQuery();
            var result = await sender.Send(query);
            return result.ToResponse();
        })        
        .WithMetadata(new
        {
            GroupName = "GetAllProducts",
            OperationId = "GetAllProducts"
        }); 
    }
}