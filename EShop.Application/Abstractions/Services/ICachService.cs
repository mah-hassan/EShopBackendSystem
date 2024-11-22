namespace EShop.Application.Abstractions.Services;

public interface ICachService
{
    Task AddOrUpdateAsync<T>(T value, string key, TimeSpan? expiryPeriod = null);
    Task<T?> GetAsync<T>(string key) where T : class;
    Task DeleteAsync(string key);
}                                                                           