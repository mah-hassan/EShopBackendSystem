using EShop.Application.Abstractions.Mappers;
using EShop.Contracts.Coupons;
using EShop.Domain.Invoices;

namespace EShop.Application.Coupons.Commands;

public sealed record CreateCouponCommand(CouponRequest Coupon)
    : ICommand<CouponResponse>;

internal sealed class CreateCouponCommandHandler(
    IStripeService stripeService,
    ICouponRepository couponRepository,
    Mapper mapper)
    : ICommandHandler<CreateCouponCommand, CouponResponse>
{
    public async Task<Result<CouponResponse>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupons = await couponRepository.GetAllAsync();

        var coupon = mapper.MapToCoupon(request.Coupon);

        var stripeCoupon = await stripeService.CreateCouponAsync(coupon);

        coupon.Code = stripeCoupon.Id;

        await couponRepository.AddOrUpdateAsync(coupon);

        return mapper.MapToCouponResponse(coupon);
    }
}