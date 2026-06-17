using System.Text.Json.Serialization;
using e_commerce_web_customer.Application.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace e_commerce_web_customer.Infrastructure.Integrations.GoogleMaps;

public sealed class GoogleMapsIntegration(
    HttpClient httpClient,
    IOptions<GoogleMapsOptions> options) : IGoogleMapsIntegration
{
    public GoogleMapsClientConfig GetClientConfig()
    {
        var browserApiKey = options.Value.GetBrowserApiKey();
        return string.IsNullOrWhiteSpace(browserApiKey)
            ? new GoogleMapsClientConfig(
                false,
                string.Empty,
                "Chưa cấu hình Google Maps API key.")
            : new GoogleMapsClientConfig(true, browserApiKey);
    }

    public async Task<GoogleMapsReverseGeocodeResult?> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var serverApiKey = options.Value.GetServerApiKey();
        if (string.IsNullOrWhiteSpace(serverApiKey))
        {
            return null;
        }

        var requestUrl =
            $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&key={Uri.EscapeDataString(serverApiKey)}&language=vi&region=vn";

        var response = await httpClient.GetFromJsonAsync<GoogleGeocodeResponse>(
            requestUrl,
            cancellationToken);

        if (response is null
            || !string.Equals(response.Status, "OK", StringComparison.OrdinalIgnoreCase)
            || response.Results.Count == 0)
        {
            return null;
        }

        var selected = response.Results.FirstOrDefault(IsVietnamResult)
            ?? response.Results[0];

        return new GoogleMapsReverseGeocodeResult(
            selected.FormattedAddress,
            selected.AddressComponents.Select(component =>
                new GoogleMapsAddressComponent(
                    component.LongName,
                    component.ShortName,
                    component.Types)).ToList());
    }

    private static bool IsVietnamResult(GoogleGeocodeResult result)
    {
        return result.AddressComponents.Any(component =>
            component.Types.Contains("country")
            && (string.Equals(component.ShortName, "VN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(component.LongName, "Việt Nam", StringComparison.OrdinalIgnoreCase)
                || string.Equals(component.LongName, "Vietnam", StringComparison.OrdinalIgnoreCase)));
    }

    private sealed class GoogleGeocodeResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("results")]
        public List<GoogleGeocodeResult> Results { get; set; } = [];
    }

    private sealed class GoogleGeocodeResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("address_components")]
        public List<GoogleAddressComponent> AddressComponents { get; set; } = [];
    }

    private sealed class GoogleAddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; } = string.Empty;

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = [];
    }
}
