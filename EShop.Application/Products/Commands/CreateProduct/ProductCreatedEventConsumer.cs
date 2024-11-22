using EShop.Application.Products.Queries.Search;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Application.Products.Commands.CreateProduct;

public sealed class ProductCreatedEventConsumer
    : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventConsumer> _logger;
    private readonly IElasticSearchService _elasticsearchService;

    public ProductCreatedEventConsumer(ILogger<ProductCreatedEventConsumer> logger, IElasticSearchService elasticsearchService)
    {
        _logger = logger;
        _elasticsearchService = elasticsearchService;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        _logger.LogInformation($"Product created event: for {context.Message.ProductName}");
        var elasticSearchProduct = new ElasticSearchProduct
        {
            Id = context.Message.Id,
            Name = context.Message.ProductName,
            Description = context.Message.ProductDescription,
            Price = context.Message.Price,
            PrimaryImage = context.Message.PrimaryImage,
            CategoryId = context.Message.CategoryId,
            BrandId = context.Message.BrandId,
            SKU = context.Message.SKU
        };
        await _elasticsearchService.AddOrUpdateAsync(elasticSearchProduct);
        _logger.LogInformation("consumed");  
    }
}