using EShop.Application.Abstractions.Mappers;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using Microsoft.Extensions.Logging;

namespace EShop.Application.Orders.Commands.CompleteCheckout;

public sealed record CompleteCheckoutCommand(Guid OrderId, string? paymentId)
    : ICommand;

internal sealed class CompleteCheckoutCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IEmailService emailService,
    ICouponRepository couponRepository,
    ILogger<CompleteCheckoutCommandHandler> logger,
    Mapper mapper,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteCheckoutCommand>
{
    public async Task<Result> Handle(CompleteCheckoutCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId);

        if (order is null)
        {
            return Result.Failure(new Error("Order", "Checkout not completed, Order was not found", ErrorType.NotFound));
        }

        var updateStockResult = await UpdateStock(order);

        if (updateStockResult.IsFailure)
            return updateStockResult;

        order.Status = OrderStatus.Shipped;

        if (string.IsNullOrEmpty(request.paymentId))
        {
            logger.LogError("PaymentId is not provided for order {order}", order.Id);
        }

        order.PaymentId = request.paymentId;

        orderRepository.Update(order);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var coupons = await couponRepository.GetAllAsync();
        var orderSummary = mapper.MapToOrderSummary(order);
        orderSummary.SetInvoice(InvoiceFactory.Create(order, coupons));
        await emailService.SendAsync(order.CustomerEmail,
            "PaymentReceived",
            "checkout-completed.cshtml",
            orderSummary);

        return Result.Success();
    }

    private async Task<Result> UpdateStock(Order order)
    {
        foreach (var item in order.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);

            if (product is null)
            {
                return Result.Failure(new Error("Product", $"Checkout not completed, Product {item.Name} was not found", ErrorType.NotFound));
            }

            product.StockQuantity -= item.Quantity;
            product.OrderedQuantity -= item.Quantity;

            productRepository.Update(product);
        }
        return Result.Success();
    }

}