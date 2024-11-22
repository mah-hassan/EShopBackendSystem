using EShop.Domain.Abstractions;
using EShop.Domain.Brands;
using EShop.Domain.Entities;
using EShop.Domain.ValueObjects;

namespace EShop.Domain.Products;

public sealed class Product : Entity
{
    public Product()
        : base(Guid.NewGuid())
    {

    }
    public required string Name { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string PrimaryImage { get; set; } = string.Empty;
    public required Money Price { get; set; }
    public required int StockQuantity { get; set; }
    public int OrderedQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public required Guid CategoryId { get; set; }
    public required Guid BrandId { get; set; }
    public Category Category { get; }
    public Brand Brand { get; }
    public List<Review> Reviews { get; set; } = new();
    public List<VariantOption> VariantOptions { get; set; }

    public IEnumerable<IGrouping<Variant, VariantOption>> Variants
        => VariantOptions.GroupBy(option => option.Variant);
}