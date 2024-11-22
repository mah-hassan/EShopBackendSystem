using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Application.Common.Extensions;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Commands.UpdateShoppingCartItem;

public sealed record UpdateShoppingCartItemCommand(ProductLineItemRequest Item,Guid ItemId)
    : ICommand<ProductLineItem>;

internal sealed class UpdateShoppingCartItemCommandHandler(
    IShoppingCartRepository shoppingCartRepository,
    IProductRepository productRepository,
    IHttpContextAccessor contextAccessor,
    ISupabaseService supabaseService,
    Mapper mapper)
    : ICommandHandler<UpdateShoppingCartItemCommand, ProductLineItem>
{
    public async Task<Result<ProductLineItem>> Handle(UpdateShoppingCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var cart = await shoppingCartRepository.GetByUserIdAsync(userId);

        if (cart is null) 
        {
            return Result.Failure<ProductLineItem>(new Error("ShoppingCart", "user have not created a Shopping cart ", ErrorType.NotFound));
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);

        if (cartItem is null)
        {
            return Result.Failure<ProductLineItem>(new Error("ShoppingCartItem",
                            "Item not found in cart",
                            ErrorType.NotFound));
        }

        var product = await productRepository.GetByIdAsync(request.Item.ProductId);

        if (product is null)
        {
            return Result
                .Failure<ProductLineItem>(new Error("Product", "Product was deleted",
                ErrorType.NotFound));
        }
    
        foreach (var variant in request.Item.Variants)
        {
            var desiredVariant = product
                .Variants
                .FirstOrDefault(v => v.Key.Name.Equals(variant.Key, StringComparison.OrdinalIgnoreCase));
         
            var added = cartItem.AddVariant(desiredVariant, variant);
            if (added.IsFailure)
            {
                return Result.Failure<ProductLineItem>(added.Errors!);
            }         
        }

        cartItem.Quantity = request.Item.Quantity;
        cartItem.UnitPrice = product.Price;
        cartItem.Name = product.Name;
        cartItem.Image = supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);

        await shoppingCartRepository.CreateOrUpdateAsync(cart);

        return mapper.MapToProductLineItem(cartItem);
    }
}