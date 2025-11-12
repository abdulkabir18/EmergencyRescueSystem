using Application.Interfaces.External;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services.GoogleMaps
{
    public class GoogleMapsGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleMapsGeocodingService> _logger;
        private readonly GoogleMapsSettings _settings;
        public GoogleMapsGeocodingService(HttpClient httpClient, ILogger<GoogleMapsGeocodingService> logger, IOptions<GoogleMapsSettings> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<ReverseGeocodeResult?> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_settings.ApiKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.GetProperty("status").GetString() != "OK")
                {
                    _logger.LogWarning("Google Geocoding API returned non-OK status: {Status}", root.GetProperty("status").GetString());
                    return null;
                }

                var result = root.GetProperty("results")[0];

                string? street = null, city = null, state = null, lga = null, country = null, postalCode = null;

                foreach (var component in result.GetProperty("address_components").EnumerateArray())
                {
                    var types = component.GetProperty("types").EnumerateArray().Select(t => t.GetString()).ToList();

                    if (types.Contains("route")) street = component.GetProperty("long_name").GetString();
                    if (types.Contains("locality")) city = component.GetProperty("long_name").GetString();
                    if (types.Contains("administrative_area_level_2")) lga = component.GetProperty("long_name").GetString();
                    if (types.Contains("administrative_area_level_1")) state = component.GetProperty("long_name").GetString();
                    if (types.Contains("country")) country = component.GetProperty("long_name").GetString();
                    if (types.Contains("postal_code")) postalCode = component.GetProperty("long_name").GetString();
                }

                return new ReverseGeocodeResult(street, city, state, lga, country, postalCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reverse geocoding coordinates ({Lat}, {Lng})", latitude, longitude);
                return null;
            }
        }
    }
}