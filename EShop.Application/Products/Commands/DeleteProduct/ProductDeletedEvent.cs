using EShop.Application.Common.Constants;
using EShop.Domain.Orders;
using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;

namespace EShop.Application.Products.Commands.DeleteProduct;

public sealed record ProductDeletedEvent
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid ProductCategoryId { get; init; }
    public Guid ProductBrandId { get; init; }
    public string PrimaryImage { get; init; } = string.Empty;
    public List<string> Images { get; init; } = new();
}
public sealed class ProductDeletedEventConsumer(
    ISupabaseService supabaseService,
    IOrderRepository orderRepository,
    IElasticSearchService elasticSearchService,
    ILogger<ProductDeletedEventConsumer> logger)
    : IConsumer<ProductDeletedEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        try
        {
            await Task.WhenAll(RemoveResourcesFromSupabase(context.Message.PrimaryImage, context.Message.Images),
                    orderRepository.RemoveProductFromOrdersAsync(context.Message.ProductId),
                    elasticSearchService.RemoveAsync(context.Message.ProductId));

        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while consuming {event} Message: {message}",
                context.Message.GetType(), context.Message);
            context.LogRetry(ex);
        }
    }


    private async Task RemoveResourcesFromSupabase(string primaryImage, List<string> images)
    {
        var deleteImagesTasks = images.Select(image => supabaseService.DeleteFileAsync(SupabaseBackets.Products, image)).ToList();
        var deletePrimaryImageTask = supabaseService.DeleteFileAsync(SupabaseBackets.Products, primaryImage);
        deleteImagesTasks.Add(deletePrimaryImageTask);
        await Task.WhenAll(deleteImagesTasks);
    }
}