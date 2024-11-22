using Bogus;
using EShop.Contracts.Orders;

namespace EShop.Test.SharedUtilities.Orders;

public static class ShippingInfoRquestFaker
{
    private static readonly Faker<ShippingInfo> shippingInfoFaker;

    static ShippingInfoRquestFaker()
    {
        shippingInfoFaker = new Faker<ShippingInfo>()
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.City, f => f.Address.City())
            .RuleFor(s => s.Region, f => f.Address.State())
            .RuleFor(s => s.PostalCode, f => f.Address.ZipCode());
    }

    public static ShippingInfo Create()
    {
        return shippingInfoFaker.Generate();
    }
}