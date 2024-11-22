using EShop.Domain.Abstractions;

namespace EShop.Domain.Brands;

public interface IBrandRepository
    : IBaseRepository<Brand>
{
    Task<bool> IsNameExists(string name);
    Task<Brand?> GetByIdIncludingProductsAsync(Guid id);
}