namespace EShop.Application.Abstractions.Services;

public interface IElasticSearchService
{
    Task<bool> AddOrUpdateAsync<T>(T entity, string? index = null);
    Task<bool> RemoveAsync(Guid documentId, string? index = null);
    Task<List<T>> SearchAsync<T>(string searchTerm, string? index = null);
}