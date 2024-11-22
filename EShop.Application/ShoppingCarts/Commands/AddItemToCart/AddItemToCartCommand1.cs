using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Application.Common.Extensions;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Commands.AddItemsToCart;

public sealed record AddItemToCartCommand(ProductLineItemRequest Item)
    : ICommand<ProductLineItem>;

internal sealed class AddItemToCartCommandHandler
    (IProductRepository productRepository,
    IShoppingCartRepository shoppingCartRepository,
    ISupabaseService supabaseService,
    Mapper mapper,
    IHttpContextAccessor contextAccessor)
    : ICommandHandler<AddItemToCartCommand, ProductLineItem>
{
    public async Task<Result<ProductLineItem>> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();
        var cart = await shoppingCartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return Result.Failure<ProductLineItem>(new Error("ShoppingCart", "user has not created a Shopping cart ", ErrorType.NotFound));
        }

        if (cart.Items.Any(i => i.ProductId == request.Item.ProductId))
        {
            return Result.Failure<ProductLineItem>(new Error("ShoppingCartItem",
                "Item already exists in cart",
                ErrorType.Conflict));
        }

        var product = await productRepository.GetByIdAsync(request.Item.ProductId);

        if (product is null)
        {
            return Result.Failure<ProductLineItem>(new Error("Product", "Product not found", ErrorType.NotFound));
        }

        var cartItem = new ShoppingCartItem();
        cartItem.ProductId = request.Item.ProductId;
        cartItem.Quantity = request.Item.Quantity;
        cartItem.UnitPrice = product.Price;
        cartItem.Image = supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);
        cartItem.Name = product.Name;

        foreach (var variant in request.Item.Variants)
        {
            var exsistingVariant = product.Variants
                .FirstOrDefault(g => g.Key.Name.Equals(variant.Key, StringComparison.OrdinalIgnoreCase));

            var added = cartItem.AddVariant(exsistingVariant, variant);

            if (added.IsFailure)
            {
                return Result.Failure<ProductLineItem>(added.Errors!);
            }
        }

        cart.Items.Add(cartItem);

        await shoppingCartRepository.CreateOrUpdateAsync(cart);

        return mapper.MapToProductLineItem(cartItem);
    }
}
