using Microsoft.AspNetCore.Http;

namespace EShop.Contracts.Brand;

public sealed class BrandRequest
{ 
    public string Name { get; set; } = string.Empty;
    public IFormFile? Image { get; set;  } 
    public string? Description { get; set; }
    public BrandRequest() {}
}