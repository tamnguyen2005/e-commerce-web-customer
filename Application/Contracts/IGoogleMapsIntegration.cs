namespace e_commerce_web_customer.Application.Contracts;

public interface IGoogleMapsIntegration
{
    GoogleMapsClientConfig GetClientConfig();

    Task<GoogleMapsReverseGeocodeResult?> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}

public sealed record GoogleMapsClientConfig(
    bool IsConfigured,
    string BrowserApiKey,
    string? Message = null);

public sealed record GoogleMapsReverseGeocodeResult(
    string FormattedAddress,
    IReadOnlyList<GoogleMapsAddressComponent> AddressComponents);

public sealed record GoogleMapsAddressComponent(
    string LongName,
    string ShortName,
    IReadOnlyList<string> Types);
