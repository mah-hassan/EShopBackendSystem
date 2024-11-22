using EShop.Application.Categories.Commands.CreateCategory;
using EShop.Contracts.Category;

namespace EShop.Api.Categories.Endpoints;

public class CreateCategoryEndpoint : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", async (ISender sender, AddCategoryRequest request) =>
        {
            var command = new CreateCategoryCommand(request);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
}
