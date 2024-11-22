
using EShop.Application.Products.Queries.Search;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Api.Products.Endpoints;

public sealed class ProductsSearchEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/search", async (ISender sender,[FromQuery] string term) =>
        {
            var query = new ProductsSearchQuery(term);
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}