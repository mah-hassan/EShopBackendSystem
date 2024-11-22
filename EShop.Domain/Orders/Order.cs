using EShop.Domain.Abstractions;

namespace EShop.Domain.Orders;

public sealed class Order
    : Entity
{
    public Order()
            : base(Guid.NewGuid())
    {
    }

    public List<OrderItem> Items { get; set; } = new();
    public ShippingInfo ShippingInfo { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid DeliveryMethodId {  get; set; }
    public DeliveryMethod DeliveryMethod { get; set; }
    public string? PaymentId { get; set; }
}