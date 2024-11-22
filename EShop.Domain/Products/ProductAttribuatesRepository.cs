namespace EShop.Domain.Products;

public interface IProductAttribuatesRepository
{
    void AddProductAttribuate(Guid productId, Guid OptionId);
    Task RemoveProductAttribuatesAsync(Guid productId);
    Task<List<ProductAttribuates>> GetProductAttributesAsync(Guid productId);
}