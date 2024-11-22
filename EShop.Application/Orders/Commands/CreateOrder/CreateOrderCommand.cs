using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Extensions;
using EShop.Contracts.Orders;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using EShop.Domain.ShoppingCarts;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(OrderRequest order)
    : ICommand<OrderSummary>;

internal sealed class CreateOrderCommandHandler
    (Mapper mapper,
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    IShoppingCartRepository shoppingCartRepository,
    IProductRepository productRepository,
    IHttpContextAccessor contextAccessor,
    IDeliveryMethodRepository deliveryMethodRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, OrderSummary>
{
    public async Task<Result<OrderSummary>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var cart = await shoppingCartRepository.GetByUserIdAsync(userId);

        if (cart is null)
        {
            return Result.Failure<OrderSummary>(new Error("ShoppingCart", "User does not have a shopping cart to checkout", ErrorType.NotFound));
        }

        if (!cart.Items.Any())
        {
            return Result.Failure<OrderSummary>(new Error("ShoppingCart", "can not create order, Shopping cart is empty", ErrorType.BadRequest));
        }

        var deliveryMethod = await deliveryMethodRepository.GetByIdAsync(request.order.DeliveryMethodId);

        if (deliveryMethod is null)
        {
            return Result.Failure<OrderSummary>(new Error("DeliveryMethod", "UnSupported Delivery method", ErrorType.NotFound));
        }

        var order = mapper.MapToOrder(request.order);

        var customerEmail = contextAccessor.GetUserEmail();

        order.CustomerEmail = customerEmail;

        order.DeliveryMethod = deliveryMethod;

        foreach (var cartItem in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(cartItem.ProductId);

            if (product is null)
            {
                return Result.Failure<OrderSummary>(new Error("Product", $"{cartItem.Name} is no longer available", ErrorType.NotFound));
            }

            if ((product.StockQuantity - product.OrderedQuantity) <= cartItem.Quantity)
            {
                return Result
                    .Failure<OrderSummary>(new Error("Product", $"{product.Name} is out of stock", ErrorType.BadRequest));
            }

            var orderItem = new OrderItem
            {
                UnitPrice = product.Price,
                Name = product.Name,
                Quantity = cartItem.Quantity,
                ProductId = cartItem.ProductId,
                Image = cartItem.Image,
                Variants = cartItem.Variants
            };

            order.Items.Add(orderItem);
            product.OrderedQuantity += cartItem.Quantity;
            productRepository.Update(product);
        }

        orderRepository.Add(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await shoppingCartRepository.DeleteAsync(userId);
        var orderSummary = mapper.MapToOrderSummary(order);

        var coupons = await couponRepository.GetAllAsync();
        orderSummary.SetInvoice(InvoiceFactory.Create(order, coupons));

        return orderSummary; 
    }
}