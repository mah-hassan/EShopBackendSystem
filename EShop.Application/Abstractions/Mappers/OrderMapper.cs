using Riok.Mapperly.Abstractions;
using EShop.Contracts.Orders;
using EShop.Domain.Orders;
using EShop.Contracts.Coupons;
using EShop.Domain.Invoices;

namespace EShop.Application.Abstractions.Mappers;

public partial class Mapper
{
    public partial Order MapToOrder(OrderRequest order);
    public partial OrderSummary MapToOrderSummary(Order order);

    public partial Coupon MapToCoupon(CouponRequest source);
    public partial CouponResponse MapToCouponResponse(Coupon source);

}