using EShop.Application.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Data;

namespace EShop.Infrastructure.Services;

internal sealed class CachService(IDistributedCache distributedCache)
    : ICachService
{
    private readonly TimeSpan _defaultExpiryPeriod = TimeSpan.FromMinutes(5);
    public async Task AddOrUpdateAsync<T>(T value, string key, TimeSpan? expiryPeriod = null)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiryPeriod ?? _defaultExpiryPeriod,
        };
        await distributedCache.SetStringAsync(key, serializedValue, options);
    }

    public Task DeleteAsync(string key)
    {
        return distributedCache.RemoveAsync(key);
    }

    public async Task<T?> GetAsync<T>(string key) 
        where T : class
    {
        var serializedValue = await distributedCache.GetStringAsync(key);


        return serializedValue is null ? null : JsonConvert.DeserializeObject<T>(serializedValue,
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
    }
}