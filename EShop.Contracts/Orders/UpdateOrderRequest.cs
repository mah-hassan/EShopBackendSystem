using EShop.Contracts.ShoppingCart;

namespace EShop.Contracts.Orders;

public sealed class UpdateOrderRequest
{
    public List<ProductLineItem> Items { get; set; } = new();
    public Guid DeliveryMethodId { get; set; }
    public ShippingInfo ShippingInfo { get; set; }
}