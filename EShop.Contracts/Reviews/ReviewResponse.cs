namespace EShop.Contracts.Reviews;

public sealed record ReviewResponse
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required int Rating { get; set; }
    public string? Comment { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? UserAvatar { get; set; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime? UpdatedAt { get; init; }
}