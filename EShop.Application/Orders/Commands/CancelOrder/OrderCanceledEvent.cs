using EShop.Domain.Orders;
using EShop.Domain.Products;
using MassTransit;
using Stripe.Climate;

namespace EShop.Application.Orders.Commands.CancelOrder;

public record OrderCanceledEvent
{
    public Guid OrderId { get; init; }
    public OrderStatus StatusBeforeCanceling { get; init; }
    public required List<OrderItem> Items { get; init; }
    public required ShippingInfo ShippingInfo { get; init; }
}

public sealed class OrderCanceledEventConsumer(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IConsumer<OrderCanceledEvent>
{
    public async Task Consume(ConsumeContext<OrderCanceledEvent> context)
    {
        foreach (var item in context.Message.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            if (product is null)
            {
                continue;
            }
            if (context.Message.StatusBeforeCanceling is OrderStatus.Shipped)
            {
                product.StockQuantity += item.Quantity;
                // notify shipping company to cancel 
            }
            else
                product.OrderedQuantity -= item.Quantity;

            productRepository.Update(product);

        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}