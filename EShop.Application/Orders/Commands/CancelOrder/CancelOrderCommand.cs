using EShop.Domain.Orders;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Orders.Commands.CancelOrder;
public sealed record CancelOrderCommand(Guid Id)
    : ICommand;

internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IStripeService stripeService,
    IEventBus eventBus,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);
        if (order is null)
        {
            return Result.Failure(new Error("Order", "Order not found", ErrorType.NotFound));
        }
        if (order.Status is OrderStatus.Canceled or OrderStatus.Completed)
        {
            return Result.Failure(new Error("Order", $"Order is already {order.Status}", ErrorType.Conflict));
        }

        if (order.PaymentId is not null)
        {
            await stripeService.RefundAsync(order.PaymentId);
        }
        var oldStatus = order.Status;
        order.Status = OrderStatus.Canceled;
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventBus.PublishAsync(new OrderCanceledEvent
        {
            Items = order.Items,
            OrderId = order.Id,
            StatusBeforeCanceling = oldStatus, 
            ShippingInfo = order.ShippingInfo,
        });
        return Result.Success();
    }
}