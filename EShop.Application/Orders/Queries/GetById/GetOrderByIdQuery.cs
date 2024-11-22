using EShop.Application.Abstractions.Mappers;
using EShop.Contracts.Orders;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Orders.Queries.GetById;

public sealed record GetOrderByIdQuery(Guid id)
    : IQuery<OrderSummary>;

internal sealed class GetOrderByIdQueryHandler
    (IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    Mapper mapper)
    : IQueryHandler<GetOrderByIdQuery, OrderSummary>
{
    private const string templete = "checkout-completed.cshtml";
    public async Task<Result<OrderSummary>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.id);

        if (order is null)
        {
            return Result.Failure<OrderSummary>(new Error("Order", "Order not found", ErrorType.NotFound));
        }

        var orderSummary = mapper.MapToOrderSummary(order);


        var coupons = await couponRepository.GetAllAsync();
        orderSummary.SetInvoice(InvoiceFactory.Create(order, coupons));
        return orderSummary;
    }
}