using EShop.Application.Categories.Queries.GetAllCategories;

namespace EShop.Api.Categories.Endpoints;

public class GetAllCategoriesEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (ISender sender) =>
        {
            var query = new GetAllCategoriesQuery();
            var result = await sender.Send(query);
            return result.ToResponse();
        });
    }
}
