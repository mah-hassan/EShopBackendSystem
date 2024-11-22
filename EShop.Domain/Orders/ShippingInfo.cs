namespace EShop.Domain.Orders;

public sealed record ShippingInfo
{
    public required string FirstName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public required string City { get; set; } 
    public required string Region { get; set; }
    public string PostalCode { get; set; } = string.Empty;
}