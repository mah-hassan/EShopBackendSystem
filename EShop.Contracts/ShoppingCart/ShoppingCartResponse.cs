using EShop.Contracts.Products;

namespace EShop.Contracts.ShoppingCart;

public sealed class ShoppingCartResponse
{
    public List<ProductLineItem> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public bool CheckedOut { get; set; }
}

public sealed class ProductLineItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public required string Name { get; set; }
    public string Image { get; set; } = string.Empty;
    public MoneyDto UnitPrice { get; set; }
    public int Quantity { get; set; } 
    public Dictionary<string, string> Variants { get; set; } = new();
}