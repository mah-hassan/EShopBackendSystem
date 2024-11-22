using EShop.Domain.Invoices;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace EShop.Infrastructure.Repositories;

internal sealed class CouponRepository : ICouponRepository
{
    private readonly IDatabase _database;
    private const string CouponsSetKey = "coupons";

    public CouponRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task AddOrUpdateAsync(Coupon coupon)
    {
        var payload = JsonConvert.SerializeObject(coupon);
        await _database.StringSetAsync(coupon.Code, payload);

        // Add coupon key to the set
        await _database.SetAddAsync(CouponsSetKey, coupon.Code);
    }

    public async Task<bool> DeleteAsync(string code)
    {
        // Remove coupon key from the set
        await _database.SetRemoveAsync(CouponsSetKey, code);

        return await _database.KeyDeleteAsync(code);
    }

    public async Task<IReadOnlyList<Coupon>> GetAllAsync()
    {
        var couponKeys = await _database.SetMembersAsync(CouponsSetKey);

        var tasks = couponKeys.Select(async key =>
        {
            var data = await _database.StringGetAsync(key.ToString());
            return data.HasValue ? JsonConvert.DeserializeObject<Coupon>(data!) : null;
        });
        
        var coupons = await Task.WhenAll(tasks);

        return coupons.Where(c => c != null).ToList().AsReadOnly()!;
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        var data = await _database.StringGetAsync(code);

        if (!data.HasValue)
            return default;

        var coupon = JsonConvert.DeserializeObject<Coupon>(data!);

        return coupon;
    }
}
