using EShop.Domain.Invoices;
using EShop.Domain.Orders;
namespace EShop.Application.Abstractions.Services;

public interface IStripeService
{
    Task<string> CreatePaymentSessionAsync(Order order, string? couponCode = null);
    Task<Stripe.Coupon> CreateCouponAsync(Coupon coupon);
    Task RefundAsync(string paymentId);
}