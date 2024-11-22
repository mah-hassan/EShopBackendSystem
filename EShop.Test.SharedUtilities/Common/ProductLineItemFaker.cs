using Bogus;
using EShop.Contracts.Products;
using EShop.Contracts.ShoppingCart;

namespace EShop.Test.SharedUtilities.Common;

public static class ProductLineItemFaker
{
    private static readonly Faker<ProductLineItem> productLineItemFaker;

    static ProductLineItemFaker()
    {
        productLineItemFaker = new Faker<ProductLineItem>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.ProductId, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
            .RuleFor(p => p.UnitPrice, f => new MoneyDto { Ammount = f.Finance.Amount(), Currency = f.Finance.Currency().Code })
            .RuleFor(p => p.Quantity, f => f.Random.Int(1, 10));

    }

    public static ProductLineItem Create()
    {
        return productLineItemFaker.Generate();
    }

    public static List<ProductLineItem> CreateList(int count = 3)
    {
        return productLineItemFaker.Generate(count);
    }
}

public static class ProductLineItemRequestFaker
{
    private static readonly Faker<ProductLineItemRequest> productLineItemRequestFaker;
    static ProductLineItemRequestFaker() 
    {
        productLineItemRequestFaker = new Faker<ProductLineItemRequest>()
                    .RuleFor(pl => pl.ProductId, f => f.Random.Guid())
                    .RuleFor(pl => pl.Quantity, f => f.Random.Int(1, 10))
                    .RuleFor(pl => pl.Variants, f => new Dictionary<string, string>
                    {
                        { "Color", f.Commerce.Color()},
                        { "Size", f.PickRandom(new string[]{"x", "xl", "l", "m", "s" })}
                    });
    }
    public static ProductLineItemRequest Create(bool includeVariants = true)
    {
        var item = productLineItemRequestFaker.Generate();
        if (!includeVariants)
            item.Variants.Clear();
        return item;
    }
}