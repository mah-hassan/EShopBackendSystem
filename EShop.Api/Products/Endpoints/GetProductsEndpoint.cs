using EShop.Application.Products.Queries.GetProducts;
using EShop.Contracts.Products;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Products.Endpoints;

public class GetProductsEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ISender sender,[FromBody] ProductsFillterdQuery request) =>
        {
            var query = new GetProductsQuery(
                categoryId: request.categoryId,
                brandId: request.brandId,
                pageNumber: request.pageNumber,
                size: request.size,
                orderBy: request.orderBy,
                orderType: request.orderType);

            var result = await sender.Send(query);
            return result.ToResponse();
        })     
        .WithMetadata(new 
        {
            GroupName = "GetFillteredProducts",
            OperationId = "Fillter Products",
            Description = "Returns a list of products filtered by category, brand, pagination, and sorting options.",

        }); 
    }
}