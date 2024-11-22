using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Application.Common.Extensions;
using EShop.Contracts.ShoppingCart;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.ShoppingCarts.Commands.CreateCart;

public sealed record CreateShoppingCartCommand(HashSet<ProductLineItemRequest> Items)
    : ICommand<ShoppingCartResponse>;

internal sealed class CreateShoppingCartCommandHandler
    (IShoppingCartRepository shoppingCartRepository,
    ISupabaseService supabaseService,
    IHttpContextAccessor httpContextAccessor,
    IProductRepository productRepository,
    Mapper mapper)
    : ICommandHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    public async Task<Result<ShoppingCartResponse>> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
    {
        var shoppingCart = new ShoppingCart();  
        shoppingCart.UserId = httpContextAccessor.GetUserId();
        if (await shoppingCartRepository.GetByUserIdAsync(shoppingCart.UserId) is not null)
        {
            return Result.Failure<ShoppingCartResponse>(new Error("Shopping Cart",
                            "this user has an exsisting Shopping Cart",
                            ErrorType.Conflict));
        }
        foreach (var item in request.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);

            if (product is null)
            {
                return Result.Failure<ShoppingCartResponse>(new Error("Product",
                    "Product not found",
                    ErrorType.NotFound));
            }

            var shoppingCartItem = new ShoppingCartItem();

            shoppingCartItem.ProductId = item.ProductId;

            shoppingCartItem.Quantity = item.Quantity;

            shoppingCartItem.Name = product.Name;

            shoppingCartItem.UnitPrice = product.Price;

            shoppingCartItem.Image = supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);
           
            foreach (var variant in item.Variants)
            {
                var desiredVariant = product.Variants.FirstOrDefault(v => v.Key.Name.Equals(variant.Key, StringComparison.OrdinalIgnoreCase));
                
                var added = shoppingCartItem.AddVariant(desiredVariant, variant);

                if (added.IsFailure)
                {
                    return Result
                        .Failure<ShoppingCartResponse>(added.Errors!);
                }
                
            }

            shoppingCart.Items.Add(shoppingCartItem);   
        }

        await shoppingCartRepository.CreateOrUpdateAsync(shoppingCart); 

        return mapper.MapToShoppingCart(shoppingCart);
    }
}