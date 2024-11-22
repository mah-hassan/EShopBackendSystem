namespace EShop.Domain.Entities;

public class CategoryBrands
{
    public required Guid CategoryId { get; init; }
    public required Guid BrandId { get; init; }
}
