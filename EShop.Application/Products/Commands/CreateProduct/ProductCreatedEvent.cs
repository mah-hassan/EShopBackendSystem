using EShop.Domain.ValueObjects;

namespace EShop.Application.Products.Commands.CreateProduct;

public sealed record ProductCreatedEvent
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; } 
    public required Money Price { get; set; }
    public string PrimaryImage { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public required string SKU { get; set; }
}