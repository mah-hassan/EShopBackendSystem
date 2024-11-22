using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Extensions;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Queries.GetUserShoppingCart;

public sealed record GetUserShoppingCartQuery
    : IQuery<ShoppingCartResponse>;
internal sealed class GetUserShoppingCartQueryHandler
    (IShoppingCartRepository shoppingCartRepository,
    IHttpContextAccessor contextAccessor,
    Mapper mapper)
    : IQueryHandler<GetUserShoppingCartQuery, ShoppingCartResponse>
{
    public async Task<Result<ShoppingCartResponse>> Handle(GetUserShoppingCartQuery request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var cart = await shoppingCartRepository.GetByUserIdAsync(userId);

        if (cart is null)
        {
            return Result.Failure<ShoppingCartResponse>(new Error("ShoppingCart", "User does not have a shopping cart", ErrorType.NotFound));
        }

        return mapper.MapToShoppingCart(cart);
    }
}