using EShop.Contracts.ShoppingCart;

namespace EShop.Contracts.Orders;

public sealed class OrderSummary
{
    public Guid Id { get; set; }
    public List<ProductLineItem> Items { get; set; } = new();
    public required DeliveryMethod DeliveryMethod { get; set; }
    public required string Status { get; set; }
    public required ShippingInfo ShippingInfo { get; set; }
    public object Invoice { get; private set; } = new();

    public void SetInvoice(object invoice)
    {
        if (invoice is not null && invoice.GetType().BaseType is not null &&
              string.Equals(invoice.GetType().BaseType?.Name, nameof(Invoice), StringComparison.Ordinal))
        {
            Invoice = invoice;
        }
        else
            throw new ArgumentException("invalid inovoice");
    }
}
public sealed record DeliveryMethod(Guid Id, string Name, string? Description);