using EShop.Domain.Abstractions;

namespace EShop.Domain.Products;

public class Review : Entity
{
    public Review() : base(Guid.NewGuid())
    {
    }

    public required Guid ProductId { get; init; }
    public Guid UserId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
}