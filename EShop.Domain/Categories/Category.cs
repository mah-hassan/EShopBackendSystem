using EShop.Domain.Abstractions;
using EShop.Domain.Brands;
using EShop.Domain.Products;

namespace EShop.Domain.Entities;

public class Category : Entity
{
    public Category()
        : base(Guid.NewGuid())
    {

    }
    public required string Name { get; set; }
    public Guid ParentCategoryId { get; set; } = Guid.Empty;
    public bool IsParentCategory { get; set; }
    public List<Variant> Variants { get; set; } = new();
    public HashSet<Brand> Brands { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}
