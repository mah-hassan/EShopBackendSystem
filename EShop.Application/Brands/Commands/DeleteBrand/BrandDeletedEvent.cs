using EShop.Application.Common.Constants;
using EShop.Application.Products.Commands.DeleteProduct;
using EShop.Domain.Brands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Application.Brands.Commands.DeleteBrand;

public sealed record BrandDeletedEvent(Guid BrandId);

public sealed class BrandDeletedEventConsumer(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ISupabaseService supabaseService,
    ILogger<BrandDeletedEventConsumer> logger)
    : IConsumer<BrandDeletedEvent>
{
    public async Task Consume(ConsumeContext<BrandDeletedEvent> context)
    {
        var brand = await brandRepository.GetByIdIncludingProductsAsync(context.Message.BrandId);
        if (brand is null || !brand.IsDeleted)
        {
            logger.LogWarning("Brand {BrandId} was not found or is not deleted", context.Message.BrandId);
            return;
        }

        var productsDeletedEvents =
            brand.Products.Select(p => new ProductDeletedEvent
            {
                PrimaryImage = p.PrimaryImage,
                Images = p.Images,
                ProductBrandId = p.BrandId,
                ProductCategoryId = p.CategoryId,
                ProductId = p.Id,
                ProductName = p.Name
            });

        var publishProductsDeletedEvents = productsDeletedEvents.Select(e => eventBus.PublishAsync(e));

        brandRepository.Delete(brand);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Brand {BrandId} has been deleted", context.Message.BrandId);
        await supabaseService.DeleteFileAsync(SupabaseBackets.Brands, brand.Image);
        await Task.WhenAll(publishProductsDeletedEvents);
        logger
            .LogInformation("product-deleted-event have been published for all brand: {brandId}`s products", context.Message.BrandId);
    }
}