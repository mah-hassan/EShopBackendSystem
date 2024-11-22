namespace EShop.Domain.Invoices;

public interface ICouponRepository
{
    Task AddOrUpdateAsync(Coupon coupon);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IReadOnlyList<Coupon>> GetAllAsync();
    Task<bool> DeleteAsync(string code);
}