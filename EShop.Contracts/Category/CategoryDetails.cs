using EShop.Contracts.Variants;

namespace EShop.Contracts.Category;

public sealed class CategoryDetails
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<VariantResponse> Variants { get; set; } = new();
}

