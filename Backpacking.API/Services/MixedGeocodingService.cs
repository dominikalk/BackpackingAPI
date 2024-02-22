using Backpacking.API.Models.Geocoding;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Newtonsoft.Json;
using System.Net;

namespace Backpacking.API.Services;

public class MixedGeocodingService : IGeocodingService
{
    private readonly IConfiguration _configuration;

    public MixedGeocodingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<IEnumerable<GeocodingLocation>>> ForwardGeocode(string query)
    {
        HttpClient client = new HttpClient();

        string? baseUrl = _configuration.GetValue<string>("BaseApiStrings:PositionStack");
        string? apiKey = _configuration["PositionStackApiKey"];

        Uri uri = new Uri($"{baseUrl}/forward?access_key={apiKey}&query={query}");

        HttpResponseMessage response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return new BPError(HttpStatusCode.InternalServerError, "Internal server error");
        }

        string apiResponse = await response.Content.ReadAsStringAsync();
        PositionStackResponse? positionStackResponse = JsonConvert.DeserializeObject<PositionStackResponse>(apiResponse);

        if (positionStackResponse is null)
        {
            return new BPError(HttpStatusCode.InternalServerError, "Internal server error");
        }

        return Result<IEnumerable<GeocodingLocation>>.Ok(
            positionStackResponse.Data.Select(location => new GeocodingLocation(location)));
    }

    public async Task<Result<IEnumerable<string>>> ReverseGeocode(float longitude, float latitude)
    {
        HttpClient client = new HttpClient();

        string? baseUrl = _configuration.GetValue<string>("BaseApiStrings:Google");
        string? apiKey = _configuration["GoogleApiKey"];

        Uri uri = new Uri($"{baseUrl}?key={apiKey}&latlng={latitude},{longitude}&result_type=locality|colloquial_area|country|sublocality|neighborhood|political");

        HttpResponseMessage response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return new BPError(HttpStatusCode.InternalServerError, "Internal server error");
        }

        string apiResponse = await response.Content.ReadAsStringAsync();
        GoogleResponse? googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(apiResponse);

        if (googleResponse is null)
        {
            return new BPError(HttpStatusCode.InternalServerError, "Internal server error");
        }

        return Result<IEnumerable<string>>.Ok(
            googleResponse.Results.Select(location => location.Formatted_Address).Distinct());
    }
}
