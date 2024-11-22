using EShop.Contracts.Reviews;

namespace EShop.Contracts.Products;

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required MoneyDto Price { get; set; }
    public string PrimaryImage { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public ReviewSummary ReviewSummary { get; set; } 
}