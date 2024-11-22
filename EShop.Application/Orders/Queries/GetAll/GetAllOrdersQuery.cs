using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Extensions;
using EShop.Contracts.Orders;
using EShop.Domain.Invoices;
using EShop.Domain.Orders;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Orders.Queries.GetAll;

public sealed record GetAllOrdersForAUserQuery
    : ICachedQuery<List<OrderSummary>>
{
    public string CachKey => "orders-all";

    public TimeSpan? Period => TimeSpan.FromMinutes(30);
}

internal sealed class GetAllOrdersForAUserQueryHandler(
    IOrderRepository orderRepository,
    IHttpContextAccessor contextAccessor,
    ICouponRepository couponRepository,
    Mapper mapper)
    : IQueryHandler<GetAllOrdersForAUserQuery, List<OrderSummary>>
{
    public async Task<Result<List<OrderSummary>>> Handle(GetAllOrdersForAUserQuery request, CancellationToken cancellationToken)
    {
        var customerEmail = contextAccessor.GetUserEmail();
        var orders = await orderRepository.GetAllByCustomerEmailAsync(customerEmail);
        var ordersList = new List<OrderSummary>();
        var coupons = await couponRepository.GetAllAsync();
        foreach (var order in orders)
        {
            var orderSummary = mapper.MapToOrderSummary(order);
            orderSummary.SetInvoice(InvoiceFactory.Create(order, coupons));
            ordersList.Add(orderSummary);
        }
        return ordersList;
    }
}
