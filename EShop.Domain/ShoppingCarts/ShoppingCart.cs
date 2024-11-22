using EShop.Domain.Abstractions;
using EShop.Domain.Products;
using EShop.Domain.Shared;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShop.Domain.ShoppingCarts;
[NotMapped]
public sealed class ShoppingCart
    : Entity
{
    public ShoppingCart()
        : base(Guid.NewGuid())
    {
    }
    public Guid UserId { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.UnitPrice.Ammount * i.Quantity);
}
[NotMapped]
public sealed class ShoppingCartItem : Entity
{
    public ShoppingCartItem() : base(Guid.NewGuid())
    {
    }

    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public string Image { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Money UnitPrice { get; set; }
    public Dictionary<string, string> Variants { get; set; } = new();


    /// <summary>
    /// Adds or Updates if exsists
    /// the <paramref name="choosedOption"/> to CartItem Variants
    /// </summary>
    /// <param name="source">the actual product variant options to check against</param>
    /// <param name="choosedOption">the chosed variant option to Buy</param>
    /// <returns></returns>
    public Result AddVariant(IGrouping<Variant, VariantOption>? source,
        KeyValuePair<string, string> choosedOption)
    {

        if (source is null)
        {
            return Result
                .Failure(new Error("Product.Variant",
                    $"Variant '{choosedOption.Key}' not found for this product",
                    ErrorType.BadRequest));
        }

        if (!source.Any(o => o.Value.Equals(choosedOption.Value, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure(new Error("Product.Variant.Option",
                    $"Option '{choosedOption.Value}' not available in this product`s {choosedOption.Key}s",
                    ErrorType.BadRequest));
        }

        if (Variants.ContainsKey(choosedOption.Key))
            Variants[choosedOption.Key] = choosedOption.Value;
        else
            Variants.Add(choosedOption.Key, choosedOption.Value);

        return Result.Success();
    }

}