using EShop.Domain.ValueObjects;

namespace EShop.Domain.Invoices;

public abstract class Invoice
{
    protected Invoice(int itemsTotalCount,
        Dictionary<string, Money> itemsPrices,
        decimal subtotal,
        decimal deliveryCost,
        decimal totalPrice)
    {
        ItemsTotalCount = itemsTotalCount;
        ItemsCosts = itemsPrices;
        Subtotal = subtotal;
        DeliveryCost = deliveryCost;
        TotalPrice = totalPrice;
    }

    public int ItemsTotalCount { get; init; }
    public Dictionary<string, Money> ItemsCosts { get; init; } 
    public decimal Subtotal { get; init; }
    public decimal DeliveryCost { get; init; }
    public decimal TotalPrice { get; init; }
}

public sealed class DiscountlessInvoice : Invoice
{
    internal DiscountlessInvoice(int itemsTotalCount,
        Dictionary<string, Money> itemsPrices,
        decimal subtotal,
        decimal deliveryCost,
        decimal totalPrice)
     : base(itemsTotalCount, itemsPrices, subtotal, deliveryCost, totalPrice)
    {
    }
}

public sealed class DiscountedInvoice : Invoice
{
    internal DiscountedInvoice(
        decimal discount,
        int itemsTotalCount,
        Dictionary<string, Money> itemsPrices,
        decimal subtotal,
        decimal deliveryCost,
        decimal totalPrice,
        Coupon coupon)
        : base(itemsTotalCount, itemsPrices, subtotal, deliveryCost, totalPrice)
    {
        Discount = discount;
        Coupon = coupon;
    }

    public Coupon Coupon { get; init; }
    public decimal Discount { get; init; }
}