using EShop.Application.Products.Queries.Search;
using EShop.Contracts.Products;
using EShop.Domain.Products;
using EShop.Domain.ValueObjects;
using MassTransit;

namespace EShop.Application.Products.Commands.UpdateProduct;

public sealed record ProductUpdatedEvent
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductDescription { get; init; }
    public Guid ProductCategoryId { get; init; }
    public Guid ProductBrandId { get; init; }
    public required Money Price { get; init; }
    public string PrimaryImage { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
}

public class ProductUpdatedEventConsumer(
    ICachService cachService,
    IElasticSearchService elasticSearchService)
    : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var elasticSearchProduct = new ElasticSearchProduct
        {
            Id = context.Message.ProductId,
            Name = context.Message.ProductName,
            Description = context.Message.ProductDescription,
            Price = context.Message.Price,
            CategoryId = context.Message.ProductCategoryId,
            BrandId = context.Message.ProductBrandId,
            PrimaryImage = context.Message.PrimaryImage,
            SKU = context.Message.SKU,
        };
        await Task.WhenAll(UpdateElasticSearch(elasticSearchProduct),
            cachService.DeleteAsync($"product-{context.Message.ProductId}"));
    }
    private Task UpdateElasticSearch(ElasticSearchProduct elasticSearchProduct)
    {
        return elasticSearchService.AddOrUpdateAsync(elasticSearchProduct);
    }
}