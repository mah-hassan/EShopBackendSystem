using EShop.Domain.Abstractions;
using EShop.Domain.Products;

namespace EShop.Domain.Brands;

public class Brand : Entity
{
    public Brand()
        : base(Guid.NewGuid())
    {

    }
    public required string Name { get; set; }
    public string Image { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public List<Product> Products { get; set; } = new();
}