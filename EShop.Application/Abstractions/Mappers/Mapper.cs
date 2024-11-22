using EShop.Contracts.Products;
using Riok.Mapperly.Abstractions;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.ValueObjects;
using EShop.Contracts.Reviews;
using EShop.Domain.Products;
using EShop.Domain.ShoppingCarts;

namespace EShop.Application.Abstractions.Mappers;

[Mapper(EnabledConversions = MappingConversionType.All)]
public partial class Mapper
{
    public partial ShoppingCartResponse MapToShoppingCart(ShoppingCart cart);

    public partial Money MapToMoney(MoneyDto price);
}