using EShop.Contracts.Brand;

namespace EShop.Contracts.Category;

public sealed record CategoryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public List<SubCategory> SubCategories { get; set; } = new();
}

public sealed class SubCategory
{
    public Guid Id { get; init; }
    public Guid ParentCategoryId { get; init; }
    public required string Name { get; init; }
    public List<BrandResponse> Brands { get; set; } = new();
}