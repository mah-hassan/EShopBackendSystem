using EShop.Domain.Abstractions;

namespace EShop.Domain.Products;

public sealed class Variant
    : Entity
{
    public Variant() : base(Guid.NewGuid())
    {
    }
    public required string Name { get; set; }
    public Guid CategoryId { get; set; }
    public List<VariantOption> Options { get; set; } = new();
}

public sealed class VariantOption : Entity
{
    public VariantOption() : base(Guid.NewGuid())
    {
    }

    public required string Value { get; set; }
    public Guid VariantId { get; set; }
    public Variant Variant { get; set; }
}
