namespace EShop.Contracts.Brand;

public sealed class BrandResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; } 
    public string Image { get; set; } = string.Empty;
    public string? Description { get; init; }
}