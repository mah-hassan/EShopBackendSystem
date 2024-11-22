using EShop.Application.Common.Extensions;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Commands.RemoveItemFromCart;

public sealed record RemoveItemFromCartCommand(Guid ItemId)
    : ICommand;

internal sealed class RemoveItemFromCartCommandHandler(
    IShoppingCartRepository shoppingCartRepository,
    IHttpContextAccessor contextAccessor)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task<Result> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var cart = await shoppingCartRepository.GetByUserIdAsync(userId);

        if (cart is null)
        {
            return Result.Failure(new Error("ShoppingCart", "Shopping cart not found", ErrorType.NotFound));
        }

        var item = cart.Items.FirstOrDefault(item => item.Id == request.ItemId);

        if (item is null)
        {
            return Result.Failure(new Error("ShoppingCartItem", "Item not found in shopping cart", ErrorType.NotFound));
        }

        cart.Items.Remove(item);

        await shoppingCartRepository.CreateOrUpdateAsync(cart);

        return Result.Success();
    }
}