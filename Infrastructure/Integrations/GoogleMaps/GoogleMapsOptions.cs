namespace e_commerce_web_customer.Infrastructure.Integrations.GoogleMaps;

public sealed class GoogleMapsOptions
{
    public const string SectionName = "GoogleMaps";

    public string ApiKey { get; set; } = string.Empty;
    public string BrowserApiKey { get; set; } = string.Empty;
    public string ServerApiKey { get; set; } = string.Empty;

    public string GetBrowserApiKey()
    {
        return FirstNonEmpty(BrowserApiKey, ApiKey);
    }

    public string GetServerApiKey()
    {
        return FirstNonEmpty(ServerApiKey, ApiKey, BrowserApiKey);
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values
            .Select(value => value?.Trim() ?? string.Empty)
            .FirstOrDefault(value => value.Length > 0)
            ?? string.Empty;
    }
}
