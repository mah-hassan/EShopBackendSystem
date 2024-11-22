namespace EShop.Domain.Invoices;

public sealed class Coupon
{
    public string Code { get; set; } = string.Empty;
    public decimal SavePercentage { get; init; }
    public decimal MinimumAmount { get; init; }
}