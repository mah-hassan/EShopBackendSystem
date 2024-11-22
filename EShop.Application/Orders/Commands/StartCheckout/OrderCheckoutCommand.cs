using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Orders.Commands.Checkout;

public sealed record StartOrderCheckoutSessionCommand(Guid Id)
    : ICommand<string>;

internal sealed class StartOrderCheckoutSessionCommandHandler(
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    IStripeService stripeService)
    : ICommandHandler<StartOrderCheckoutSessionCommand, string>
{
    public async Task<Result<string>> Handle(StartOrderCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);

        if (order is null)
        {
            return Result.Failure<string>(new Error("Order", "Order Not Found", ErrorType.NotFound));
        }

        if (order.Status is not OrderStatus.Placed)
        {
            return Result.Failure<string>(new Error("Order.Status", $"Order have been {order.Status}", ErrorType.BadRequest));
        }

        var coupons = await couponRepository.GetAllAsync();

        var invoice = InvoiceFactory.Create(order, coupons);
        
        var coupon = invoice is DiscountedInvoice discountedInvoice ? discountedInvoice.Coupon.Code : null;

        var sessionUrl = await stripeService.CreatePaymentSessionAsync(order, coupon);

        return sessionUrl;    
    }
}