using EShop.Domain.Orders;
using EShop.Domain.ValueObjects;

namespace EShop.Domain.Invoices;

public sealed class InvoiceFactory
{
    public static Invoice Create(Order order, IReadOnlyList<Coupon>? coupons = null)
    {
        var itemsPrices = new Dictionary<string, Money>();

        decimal subtotal = 0;

        int itemsTotalCount = 0;

        foreach (var item in order.Items)
        {
            itemsPrices.Add(item.Name, item.UnitPrice * item.Quantity);
            subtotal += item.UnitPrice.Ammount;
            itemsTotalCount += item.Quantity;
        }

        decimal total = subtotal + order.DeliveryMethod.DeliveryCost;

        if (coupons is null || !coupons.Any())
        {
            return new DiscountlessInvoice(
              itemsTotalCount,
              itemsPrices,
              subtotal,
              order.DeliveryMethod.DeliveryCost,
              total);
        }
        else
        {
            var coupon = coupons
                .Where(c => c.MinimumAmount <= total)
                .OrderBy(c => total - c.MinimumAmount).First();

            decimal discount = total * (coupon.SavePercentage / 100);

            return new DiscountedInvoice(discount,
              itemsTotalCount,
              itemsPrices,
              subtotal,
              order.DeliveryMethod.DeliveryCost,
              total -= discount,
              coupon);
        } 
    }
}