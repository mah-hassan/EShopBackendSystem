using Microsoft.AspNetCore.Http;

namespace EShop.Contracts.Products;

public sealed class ProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? PrimaryImage { get; set; } 
    public IFormFileCollection Images { get; set; }
    public int StockQuantity { get; set; }
    public MoneyDto Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public List<Guid> Attribuates { get; set; } = new();
}
public record MoneyDto
{
    public decimal Ammount { get; set; }
     
    public string Currency { get; set; }
    public MoneyDto(decimal ammount, string currency)
    {
        Ammount = ammount;
        Currency = currency;
    }
    public MoneyDto()
    {
    }
}