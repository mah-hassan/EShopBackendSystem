
using EShop.Application.Categories.Commands.DeleteCategory;

namespace EShop.Api.Categories.Endpoints;

public sealed class DeleteCategoryEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{id}", async (ISender sender, Guid id) =>
        {
            var command = new DeleteCategoryCommand(id);
            var result = await sender.Send(command);
            return result.ToResponse();
        });       
    }
}