namespace EShop.Contracts.Coupons;

public sealed record CouponRequest(decimal SavePercentage, decimal MinimumAmount);
public sealed record CouponResponse(string Code, decimal SavePercentage);