namespace EShop.Domain.ValueObjects;

public sealed record Money
{

    public decimal Ammount { get; set; }
    public string Currency { get; set; } = "USD";

    public static Money operator * (Money money, int quantity)
    {
        money.Ammount *= quantity;
        return money;
    }
}
