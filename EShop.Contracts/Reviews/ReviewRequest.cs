namespace EShop.Contracts.Reviews;

public sealed record ReviewRequest
{
    public required Guid ProductId { get; init; }
    public required int Rating { get; set; }
    public string? Comment { get; init; }
}