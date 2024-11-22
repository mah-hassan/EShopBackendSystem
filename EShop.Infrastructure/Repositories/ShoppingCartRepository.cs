using EShop.Domain.ShoppingCarts;
using Newtonsoft.Json;
using StackExchange.Redis;
namespace EShop.Infrastructure.Repositories;

internal sealed class ShoppingCartRepository
    : IShoppingCartRepository
{
    private readonly IDatabase _database;

    public ShoppingCartRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task CreateOrUpdateAsync(ShoppingCart cart)
    {
        var key = cart.UserId.ToString();

        var data = JsonConvert.SerializeObject(cart);

        await _database.StringSetAsync(key, data, TimeSpan.FromDays(30));
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var key = userId.ToString();
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(Guid userId)
    {
        var key = userId.ToString();

        if (!await _database.KeyExistsAsync(key))
        {
            return null;
        }

        var data = await _database.StringGetAsync(key);

        if (data.HasValue)
        {
            return JsonConvert.DeserializeObject<ShoppingCart>(data);
        }

        return null;
    } 
}