namespace EShop.Domain.Settings;

public sealed record  MessageBrockerSettings
{
    public const string ConfigurationSection = "MessageBrocker";
    public string Host { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}