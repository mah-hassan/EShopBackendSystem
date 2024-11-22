
using EShop.Application.Coupons.Commands;
using EShop.Contracts.Coupons;

namespace EShop.Api.Coupons.Endpoints;

public sealed class CreateCouponEndpoint
    : IEndpoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapPost("/coupons", async (ISender sender, CouponRequest coupon) =>
        {
            var command = new CreateCouponCommand(coupon);
            var result = await sender.Send(command);
            return result.ToResponse();
        });
    }
    
}