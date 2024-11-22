using Bogus;
using EShop.Domain.Brands;
using EShop.Test.SharedUtilities.Products;

namespace EShop.Test.SharedUtilities.Brands;

public static class BrandFaker
{
    private static readonly Faker<Brand> brandFaker;
    static BrandFaker()
    {
        Randomizer.Seed = new Random(5245);
        brandFaker = new Faker<Brand>()
                    .RuleFor(b => b.Id, f => f.Random.Guid())
                    .RuleFor(b => b.Name, f => f.Company.CompanyName())
                    .RuleFor(b => b.Description, f => f.Lorem.Paragraph())
                    .RuleFor(b => b.Image, (f, b) => f.Internet.UrlWithPath("https", "supabase.com", ".png"));
    }

    public static Brand Create(bool isDeleted = false)
    {
        var brand = brandFaker.Generate();
        brand.IsDeleted = isDeleted;

        return brand;
    }

    public static List<Brand> CreateList(int count)
    {

        return brandFaker.Generate(count);
    }

    public static Brand WithProducts(this Brand brand)
    {
        brand.Products = ProductFaker.GetListOfProducts(5);
        return brand;
    }
}