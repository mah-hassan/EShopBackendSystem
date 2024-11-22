using Bogus;
using EShop.Contracts.Orders;
using EShop.Test.SharedUtilities.Common;

namespace EShop.Test.SharedUtilities.Orders;

public static class UpdateOrderRequestFaker
{
    private static readonly Faker<UpdateOrderRequest> updateOrderRequestFaker;

    static UpdateOrderRequestFaker()
    {
        updateOrderRequestFaker = new Faker<UpdateOrderRequest>()
            .RuleFor(u => u.DeliveryMethodId, f => f.Random.Guid())
            .RuleFor(u => u.ShippingInfo, f => ShippingInfoRquestFaker.Create())
            .RuleFor(u => u.Items, f => ProductLineItemFaker.CreateList());
    }

    public static UpdateOrderRequest Create()
    {
        return updateOrderRequestFaker.Generate();
    }
}
