namespace EShop.Contracts.Category;

public sealed record AddCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid ParentCategoryId { get; set; }
    public bool IsParentCategory { get; set; }
    public List<VariantRequest> Variants { get; set; } = new();

}

public sealed class VariantRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}