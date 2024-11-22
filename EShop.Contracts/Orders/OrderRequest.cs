using EShop.Contracts.ShoppingCart;

namespace EShop.Contracts.Orders;

public sealed class OrderRequest
{
    public Guid DeliveryMethodId { get; set; }
    public required ShippingInfo ShippingInfo { get; set; }
}

public sealed class ShippingInfo
{
    public required string FirstName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public required string City { get; set; }
    public required string Region { get; set; }
    public string PostalCode { get; set; } = string.Empty;
}