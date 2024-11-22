using EShop.Domain.Abstractions;

namespace EShop.Domain.Orders;

public sealed class DeliveryMethod : Entity
{

    public DeliveryMethod()
            : base(Guid.NewGuid())
    {
    }
    public required string Name { get; set; }

    public string? Description { get; set; }
    public decimal DeliveryCost { get; set; }
}
