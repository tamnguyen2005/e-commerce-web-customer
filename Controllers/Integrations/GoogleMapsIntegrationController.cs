using e_commerce_web_customer.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce_web_customer.Controllers.Integrations;

[ApiController]
[Route("api/integrations/google-maps")]
public sealed class GoogleMapsIntegrationController(
    IGoogleMapsIntegration googleMapsIntegration) : ControllerBase
{
    [HttpGet("config")]
    public ActionResult<GoogleMapsClientConfigResponse> GetConfig()
    {
        var config = googleMapsIntegration.GetClientConfig();
        return new GoogleMapsClientConfigResponse(
            config.IsConfigured,
            config.BrowserApiKey,
            config.Message);
    }

    [HttpGet("reverse-geocode")]
    public async Task<ActionResult<GoogleMapsReverseGeocodeResponse>> ReverseGeocode(
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken cancellationToken)
    {
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
        {
            return BadRequest(new GoogleMapsReverseGeocodeResponse(
                false,
                null,
                "Tọa độ không hợp lệ."));
        }

        var result = await googleMapsIntegration.ReverseGeocodeAsync(
            lat,
            lng,
            cancellationToken);

        return result is null
            ? NotFound(new GoogleMapsReverseGeocodeResponse(
                false,
                null,
                "Không tìm thấy địa chỉ cho vị trí đã chọn."))
            : new GoogleMapsReverseGeocodeResponse(true, result);
    }
}

public sealed record GoogleMapsClientConfigResponse(
    bool IsConfigured,
    string ApiKey,
    string? Message);

public sealed record GoogleMapsReverseGeocodeResponse(
    bool Success,
    GoogleMapsReverseGeocodeResult? Result,
    string? Message = null);
