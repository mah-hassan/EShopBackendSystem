namespace EShop.Domain.ShoppingCarts;

public interface IShoppingCartRepository
{
    Task CreateOrUpdateAsync(ShoppingCart cart);
    Task<ShoppingCart?> GetByUserIdAsync(Guid userId);
    Task<bool> DeleteAsync(Guid userId);
}