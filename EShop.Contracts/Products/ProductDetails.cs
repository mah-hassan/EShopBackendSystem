using EShop.Contracts.Reviews;
using EShop.Contracts.Variants;

namespace EShop.Contracts.Products;

public sealed class ProductDetails
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string PrimaryImage { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
    public required MoneyDto Price { get; set; }
    public int StockQuantity { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public List<VariantResponse> Variants { get; set; } = new();
    public ReviewSummary ReviewSummary { get; set; } = new();
}