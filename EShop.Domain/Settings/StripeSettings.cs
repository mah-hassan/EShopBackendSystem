namespace EShop.Domain.Settings;


public class StripeSettings
{
    public required string Publishablekey { get; set; }
    public required string SecretKey { get; set; }
    public required string WebhookSecret { get; set; }
}
