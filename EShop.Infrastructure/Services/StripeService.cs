using EShop.Application.Abstractions.Services;
using EShop.Domain.Orders;
using EShop.Domain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace EShop.Infrastructure.Services;

internal sealed class StripeService : IStripeService
{
    private readonly IHttpContextAccessor contextAccessor;
    public StripeService(IOptions<StripeSettings> stripeSettings, IHttpContextAccessor contextAccessor)
    {
        StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
        this.contextAccessor = contextAccessor;
    }



    public async Task RefundAsync(string paymentId)
    {
        var options = new RefundCreateOptions()
        {
            PaymentIntent = paymentId,
        };
        RefundService service = new RefundService();
        await service.CreateAsync(options);
    }

    public async Task<Stripe.Coupon> CreateCouponAsync(Domain.Invoices.Coupon coupon)
    {
        var options = new CouponCreateOptions
        {
             PercentOff = coupon.SavePercentage,
             Duration = "forever",             
        };

        CouponService service = new CouponService();

        return await service.CreateAsync(options);
    }


    public async Task<string> CreatePaymentSessionAsync(Order order, string? couponCode = null)
    {
        var lineItems = new List<SessionLineItemOptions>();

        foreach (var item in order.Items)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)item.UnitPrice.Ammount * 100,
                    Currency = item.UnitPrice.Currency, 
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name,
                        Images =  new()
                        {
                            item.Image,
                        },                  
                    },
                },
                
                Quantity = item.Quantity,
            });
        }
       
        var options = new SessionCreateOptions
        {
            CustomerEmail = order.CustomerEmail,
            Metadata = new()
            {
                { "orderId", order.Id.ToString() }
            },
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Discounts = new List<SessionDiscountOptions>()
            {
                new()
                {
                    Coupon = couponCode
                },
            },
            Mode = "payment",
            SuccessUrl = $"{contextAccessor.HttpContext?.Request.Scheme}" +
            $"://{contextAccessor.HttpContext?.Request.Host}/payment-success",

        };
        var sessionService = new SessionService();
        Session session = await sessionService.CreateAsync(options);
        return session.Url;
    }
}