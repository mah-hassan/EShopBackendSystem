namespace EShop.Contracts.Reviews;

public sealed record ReviewSummary
{
    public float AverageRating { get; init; }
    public int Count { get; init; }
}