using Bogus;
using EShop.Domain.Orders;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.SharedUtilities.Orders;

public static class OrderFacker
{
    private static readonly Faker<Order> orderFaker;
    static OrderFacker()
    {
        orderFaker = new Faker<Order>()
            .RuleFor(o => o.DeliveryMethod, f => DeliveryMethodFaker.Create())
            .RuleFor(o => o.DeliveryMethodId, (f, o) => o.DeliveryMethod.Id)
            .RuleFor(o => o.Items, (f, o) =>
            {

                var items = new List<OrderItem>();
                for (int i = 0; i < f.Random.Int(1, 5); i++)
                {
                    var product = ProductFaker.CreateTestProduct();
                    items.Add
                    (
                        new()
                        {
                            ProductId = product.Id,
                            Quantity = f.Random.Int(1, 10),
                            UnitPrice = product.Price,
                            Image = product.PrimaryImage,
                            Name = product.Name,
                            OrderId = o.Id
                        }
                    );

                }
                return items;
            })
            .RuleFor(o => o.ShippingInfo, f =>
            {
                var info = ShippingInfoRquestFaker.Create();
                var shippingInfo = new Domain.Orders.ShippingInfo
                {
                    City = info.City,
                    Region = info.Region,
                    PostalCode = info.PostalCode,
                    Email = info.Email,
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    Phone = info.Phone
                };
                return shippingInfo;

            })
            .RuleFor(o => o.CustomerEmail, f => f.Internet.Email())
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>());
    }

    public static Order CreateTestOrder(OrderStatus? status = null)
    {
        Order fakeOrder = orderFaker.Generate();
        fakeOrder.Status = status ?? fakeOrder.Status;
        return fakeOrder;
    }

    public static Order ShouldHasItem(this Order order, OrderItem item)
    {
        order.Items.Add(item);
        return order;
    }
    public static List<Order> CreateList(int count = 5)
    {
        return orderFaker.Generate(count);
    }
}

public static class DeliveryMethodFaker
{
    private static readonly Faker<DeliveryMethod> deliveryMethodFaker;

    static DeliveryMethodFaker()
    {
        deliveryMethodFaker = new Faker<DeliveryMethod>()
            .RuleFor(dm => dm.Id, f => f.Random.Guid())
            .RuleFor(dm => dm.Name, f => f.Company.CompanyName())
            .RuleFor(dm => dm.Description, f => f.Lorem.Paragraph())
            .RuleFor(dm => dm.DeliveryCost, f => f.Finance.Amount());
    }

    public static DeliveryMethod Create()
    {
        DeliveryMethod fakeDeliveryMethod = deliveryMethodFaker.Generate();
        return fakeDeliveryMethod;
    }
}