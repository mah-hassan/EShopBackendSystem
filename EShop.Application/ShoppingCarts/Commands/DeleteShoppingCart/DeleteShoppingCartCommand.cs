using EShop.Application.Common.Extensions;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Commands.DeleteShoppingCart;

public sealed record DeleteShoppingCartCommand()
    : ICommand;

internal sealed class DeleteShoppingCartCommandHandler
    (IHttpContextAccessor contextAccessor,
    IShoppingCartRepository shoppingCartRepository)
    : ICommandHandler<DeleteShoppingCartCommand>
{
    public async Task<Result> Handle(DeleteShoppingCartCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var shoppingCart = await shoppingCartRepository.GetByUserIdAsync(userId);

        if (shoppingCart is null)
        {
            return Result.Failure(new Error("ShoppingCart", "Shopping cart not found", ErrorType.NotFound));
        }

        var deleted = await shoppingCartRepository.DeleteAsync(userId);

        if (deleted is false)
        {
            return Result.Failure(new Error("ShoppingCart", "Failed to delete shopping cart", ErrorType.InternalServerError));
        }

        return Result.Success();
    }
}