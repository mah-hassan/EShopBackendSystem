using EShop.Domain.ValueObjects;

namespace EShop.Application.Products.Queries.Search;

public sealed record ElasticSearchProduct
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Money Price { get; set; }
    public string PrimaryImage { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public required string SKU { get; init; } 

}