using Bogus;
using EShop.Contracts.Brand;

namespace EShop.Test.SharedUtilities.Brands;

public static class BrandRequestFaker
{
    private static readonly Faker<BrandRequest> brandRequestFaker;
    static BrandRequestFaker()
    {

        Randomizer.Seed = new Random(4554);
        brandRequestFaker = new Faker<BrandRequest>()
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(b => b.Image, (f, b) => ImageGeneratorUtility.CreateFormFile(b.Description!, $"{b.Name}.png"));


    }

    public static BrandRequest Create() => brandRequestFaker.Generate();
}
