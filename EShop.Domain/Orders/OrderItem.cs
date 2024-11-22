using EShop.Domain.Abstractions;
using EShop.Domain.Products;
using EShop.Domain.Shared;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ValueObjects;

namespace EShop.Domain.Orders;

public sealed class OrderItem
    : Entity
{
    public OrderItem()
        : base(Guid.NewGuid())
    {
    }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Dictionary<string, string> Variants { get; set; } = new();
    public required Money UnitPrice { get; set; }
    public Guid OrderId { get; set; }
    /// <summary>
    /// Adds or Updates if exsists
    /// the <paramref name="chosedOption"/> to CartItem Variants
    /// </summary>
    /// <param name="source">the actual product variant options to check against</param>
    /// <param name="chosedOption">the chosed variant option to Buy</param>
    /// <returns></returns>
    public Result AddVariant(IGrouping<Variant, VariantOption>? source,
        KeyValuePair<string, string> chosedOption)
    {

        if (source is null)
        {
            return Result
                .Failure(new Error("Product",
                    $"Variant '{chosedOption.Key}' not found for this product",
                    ErrorType.BadRequest));
        }

        if (!source.Any(o => o.Value.Equals(chosedOption.Value, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure(new Error("Product",
                    $"Variant '{chosedOption.Value}' not found for this product",
                    ErrorType.NotFound));
        }

        if (Variants.ContainsKey(chosedOption.Key))
            Variants[chosedOption.Key] = chosedOption.Value;
        else
            Variants.Add(chosedOption.Key, chosedOption.Value);

        return Result.Success();
    }


}
