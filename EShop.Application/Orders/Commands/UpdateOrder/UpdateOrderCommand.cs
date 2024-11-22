using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Contracts.Orders;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Orders.Commands.UpdateOrder;

public sealed record UpdateOrderCommand(Guid Id, UpdateOrderRequest UpdatedOrder)
    : ICommand<OrderSummary>;

internal sealed class UpdateOrderCommandHandler
    (Mapper mapper,
    IOrderRepository orderRepository,
    IDeliveryMethodRepository deliveryMethodRepository,
    IProductRepository productRepository,
    ICouponRepository couponRepository,
    ISupabaseService supabaseService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrderCommand, OrderSummary>
{
    public async Task<Result<OrderSummary>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);

        if (order is null)
        {
            return Result.Failure<OrderSummary>(new Error("Order", "Order not found", ErrorType.NotFound));
        }

        if (order.Status is not OrderStatus.Placed)
        {
            return Result.Failure<OrderSummary>(new Error("Order", $"can not update a {nameof(order.Status)} order", ErrorType.BadRequest));
        }
        // Update the order DeliveryMethod
        if (order.DeliveryMethodId != request.UpdatedOrder.DeliveryMethodId)
        {
            var deliveryMethod = await deliveryMethodRepository.GetByIdAsync(request.UpdatedOrder.DeliveryMethodId);
            if (deliveryMethod is null)
            {
                return Result.Failure<OrderSummary>(new Error("DeliveryMethod", "UnSupported Delivery method", ErrorType.NotFound));
            }
            order.DeliveryMethodId = deliveryMethod.Id;
            order.DeliveryMethod = deliveryMethod;
        }
        // Update the order Items

        var items = order.Items;

        foreach (var updatedItem in request.UpdatedOrder.Items)
        {
            var product = await productRepository.GetByIdAsync(updatedItem.ProductId);

            if (product is null)
            {
                return Result.Failure<OrderSummary>(new Error("Product", $"Product is no longer available", ErrorType.NotFound));
            }

            var existingItem = items.FirstOrDefault(i => i.ProductId == updatedItem.ProductId);

            if (existingItem is null)
            {
                return Result.Failure<OrderSummary>(new Error("Item", $"the order does not contain {updatedItem.Name} item", ErrorType.NotFound));
            }

            var quantity = updatedItem.Quantity - existingItem.Quantity;


            if ((product.StockQuantity - product.OrderedQuantity) <= updatedItem.Quantity)
            {
                return Result
                    .Failure<OrderSummary>(new Error("Product", $"{product.Name} is out of stock", ErrorType.NotFound));
            }

          
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.UnitPrice = product.Price;
            existingItem.Name = product.Name;
            existingItem.Image = supabaseService.GetPublicUrl(SupabaseBackets.Products, product.PrimaryImage);

            foreach (var variant in updatedItem.Variants)
            {
                var desiredVariant = product
                    .Variants
                    .First(v => v.Key.Name.Equals(variant.Key, StringComparison.OrdinalIgnoreCase));

                var added = existingItem.AddVariant(desiredVariant, variant);
                if (added.IsFailure)
                {
                    return Result.Failure<OrderSummary>(added.Errors!);
                }
            }
            product.OrderedQuantity += quantity;
            productRepository.Update(product);

        }

        // Update the order ShippingAddress
        UpdateShippingAddress(request.UpdatedOrder, order);

        orderRepository.Update(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var orderSummary = mapper.MapToOrderSummary(order);

        var coupons = await couponRepository.GetAllAsync();
        orderSummary.SetInvoice(InvoiceFactory.Create(order, coupons));

        return orderSummary;
    }

    private static void UpdateShippingAddress(UpdateOrderRequest UpdatedOrder, Order order)
    {
        order.ShippingInfo.FirstName = UpdatedOrder.ShippingInfo.FirstName;
        order.ShippingInfo.LastName = UpdatedOrder.ShippingInfo.LastName;
        order.ShippingInfo.Email = UpdatedOrder.ShippingInfo.Email;
        order.ShippingInfo.Phone = UpdatedOrder.ShippingInfo.Phone;
        order.ShippingInfo.PostalCode = UpdatedOrder.ShippingInfo.PostalCode;
        order.ShippingInfo.City = UpdatedOrder.ShippingInfo.City;
        order.ShippingInfo.Region = UpdatedOrder.ShippingInfo.Region;
    }
}