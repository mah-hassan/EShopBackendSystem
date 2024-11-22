namespace EShop.Domain.Settings;

public sealed class ElasticSearchSettings
{
    public const string ConfigurationSection = "ElasticSearch";
    public string Host { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = string.Empty;
}
