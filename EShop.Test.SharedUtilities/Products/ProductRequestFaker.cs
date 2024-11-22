using Bogus;
using EShop.Contracts.Products;
using Microsoft.AspNetCore.Http.Internal;

namespace EShop.Test.SharedUtilities.Products;

public static class ProductRequestFaker
{

    private static readonly Faker<ProductRequest> productRequestFaker;

    static ProductRequestFaker()
    {
        Randomizer.Seed = new Random(5252);
        productRequestFaker = new Faker<ProductRequest>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.CategoryId, f => f.Random.Guid())
            .RuleFor(p => p.BrandId, f => f.Random.Guid())
            .RuleFor(p => p.Attribuates, f => Enumerable.Range(1, 3).Select(i => f.Random.Guid()).ToList())
            .RuleFor(p => p.Price, f =>
            {
                return new Faker<MoneyDto>()
                        .RuleFor(m => m.Ammount, f => f.Finance.Amount())
                        .RuleFor(m => m.Currency, f => f.Finance.Currency().Code);

            })
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(10, 100))
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
            .RuleFor(p => p.PrimaryImage, _ => ImageGeneratorUtility.CreateFormFile("Text Product", "test-product.png"))
            .RuleFor(p => p.Images, _ => new FormFileCollection());

    }

    public static ProductRequest CreateProductRequest()
    {
        return productRequestFaker.Generate();
    }

}

public static class UpdateProductRequestFaker
{
    private static readonly Faker<UpdateProductRequest> updateProductRequestFaker;
    static UpdateProductRequestFaker()
    {
        updateProductRequestFaker = new Faker<UpdateProductRequest>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f =>
            {
                return new Faker<MoneyDto>()
                        .RuleFor(m => m.Ammount, f => f.Finance.Amount())
                        .RuleFor(m => m.Currency, f => f.Finance.Currency().Code);
            })
            .RuleFor(p => p.Attribuates, f => Enumerable.Range(1, 5).Select(i => f.Random.Guid()).ToList())
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(10, 200))
            .RuleFor(p => p.PrimaryImage, (f, request) => ImageGeneratorUtility.CreateFormFile(request.Name, $"{request.Name}.png"));
    }
    public static UpdateProductRequest CreateUpdateProductRequest()
    {
        return updateProductRequestFaker.Generate();
    }
}