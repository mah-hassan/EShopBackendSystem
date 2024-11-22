using EShop.Domain.Abstractions;
using EShop.Domain.Abstractions.Specifications;

namespace EShop.Domain.Products;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<int> CountAsync(Specification<Product> spc);
    Task<List<Product>> GetProductsWithSpecificationAsync(Specification<Product> specification, CancellationToken cancellationToken);
    Task<bool> IsSkuUnique(string sku);
}