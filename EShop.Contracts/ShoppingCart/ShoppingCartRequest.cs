namespace EShop.Contracts.ShoppingCart;

public sealed record ProductLineItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Dictionary<string, string> Variants { get; set; } = new();
}