namespace EShop.Contracts.Variants;

public sealed class VariantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Option> Options { get; set; } = new();
}

public sealed class Option
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
}