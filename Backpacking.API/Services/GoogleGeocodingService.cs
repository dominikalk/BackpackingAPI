using Backpacking.API.Models.Geocoding;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Newtonsoft.Json;
using System.Net;

namespace Backpacking.API.Services;

public class GoogleGeocodingService : IGeocodingService
{
    private readonly string? _baseUrl;
    private readonly string? _apiKey;

    public GoogleGeocodingService(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("BaseApiStrings:Google");
        _apiKey = configuration["GoogleApiKey"];
    }

    public async Task<Result<IEnumerable<GeocodingLocation>>> ForwardGeocode(string query)
    {
        HttpClient client = new HttpClient();

        Uri uri = new Uri($"{_baseUrl}?key={_apiKey}&address={query}");

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

        return Result<IEnumerable<GeocodingLocation>>.Ok(
            googleResponse.Results.Select(location => new GeocodingLocation(location)));
    }

    public async Task<Result<IEnumerable<string>>> ReverseGeocode(float longitude, float latitude)
    {
        HttpClient client = new HttpClient();

        Uri uri = new Uri($"{_baseUrl}?key={_apiKey}&latlng={latitude},{longitude}&result_type=locality|colloquial_area|country|sublocality|neighborhood|political");

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
