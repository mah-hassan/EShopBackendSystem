using Microsoft.AspNetCore.Http;

namespace EShop.Contracts.Products;

public sealed class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? PrimaryImage { get; set; }
    public int StockQuantity { get; set; }
    public MoneyDto Price { get; set; }
    public List<Guid> Attribuates { get; set; } = new();
}