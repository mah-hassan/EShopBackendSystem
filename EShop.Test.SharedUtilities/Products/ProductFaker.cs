using Bogus;
using EShop.Domain.Products;
using EShop.Domain.ValueObjects;

namespace EShop.Test.SharedUtilities.Products;

public static class ProductFaker
{

    private static readonly Faker<Product> productFaker;
    static ProductFaker()
    {

        Randomizer.Seed = new Random(58400);
        productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.Price, f =>
            {
                return new Faker<Money>().RuleFor(p => p.Ammount, f => f.Finance.Amount(100m, 10000m, 2))
                .RuleFor(p => p.Currency, f => f.Finance.Currency().Code);
            })
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(1, 1000))
            .RuleFor(p => p.BrandId, f => f.Random.Guid())
            .RuleFor(p => p.CategoryId, f => f.Random.Guid())
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
            .RuleFor(p => p.PrimaryImage, f => f.System.FileName(".png"))
            .RuleFor(p => p.VariantOptions, (f, p) =>
            {
                var variants = new List<Variant>()
                {
                    new Variant()
                    {
                        Name = "Color",
                        CategoryId = f.Random.Guid(),

                    },
                    new Variant()
                    {
                        Name = "Size",
                        CategoryId = f.Random.Guid(),

                    }
                };
                var variantOptions = new List<VariantOption>()
                {
                    new VariantOption()
                    {
                        Value = f.Commerce.Color(),
                        Variant = variants[0],
                        VariantId = variants[0].Id
                    },
                    new VariantOption()
                    {
                        Value = f.PickRandom(new string[]{"x", "xl", "l" }),
                        Variant = variants[1],
                        VariantId = variants[1].Id
                    }
                };
                return variantOptions;
            });
    }
    public static Product CreateTestProduct(
        int? stockQuantity = null,
        int? orderedQuantity = null,
        Guid? brandId = null,
        Guid? categoryId = null,
        string? name = null,
        bool includeVariants = true)
    {
        var product = productFaker.Generate();
        product.BrandId = brandId ?? product.BrandId;
        product.CategoryId = categoryId ?? product.CategoryId;
        product.Name = name ?? product.Name;
        product.StockQuantity = stockQuantity ?? product.StockQuantity;
        product.OrderedQuantity = orderedQuantity ?? product.OrderedQuantity;
        if (!includeVariants)
        {
            product.VariantOptions = new List<VariantOption>();
        }
        return product;
    }

    public static Product WithReviews(this Product product, int reviewCount)
    {
        var random = new Random();
        product.Reviews = Enumerable.Range(1, reviewCount).Select(_ => new Review()
        {
            ProductId = product.Id,
            Rating = random.Next(1, 5)
        }).ToList();
        return product;
    }
    public static List<Product> GetListOfProducts(int count = 5) 
    {
        return productFaker.Generate(count);
    }
}