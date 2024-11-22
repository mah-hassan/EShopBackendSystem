using Bogus;
using EShop.Domain.ShoppingCarts;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.SharedUtilities.ShoppingCarts;

public static class ShoppingCartFaker
{
    private static readonly Faker<ShoppingCart> shoppingCartFaker;

    static ShoppingCartFaker()
    {
        shoppingCartFaker = new Faker<ShoppingCart>()
            .RuleFor(c => c.Items, f => Enumerable.Range(1, 3).Select(i =>
            {
                var product = ProductFaker.CreateTestProduct();
                return new ShoppingCartItem()
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Image = product.PrimaryImage,
                    Quantity = f.Random.Int(1,5),
                    UnitPrice = product.Price,
                    Variants = new Dictionary<string, string>
                    {
                        { "Color", f.Commerce.Color()},
                        { "Size", f.PickRandom(new string[]{"x", "xl", "l", "m", "s" })}
                    }
                }; 
            }).ToList())
            .RuleFor(c => c.UserId, f => f.Random.Guid());
    }


    public static ShoppingCart Create()
    {
        return shoppingCartFaker.Generate();
    }

}