﻿using Backpacking.API.Models.Geocoding;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Newtonsoft.Json;

namespace Backpacking.API.Services;

public class PositionStackGeocodingService : IGeocodingService
{
    private readonly string? _baseUrl;
    private readonly string? _apiKey;

    public PositionStackGeocodingService(IConfiguration configuration)
    {
        _baseUrl = configuration.GetValue<string>("BaseApiStrings:PositionStack");
        _apiKey = configuration["PositionStackApiKey"];
    }

    public async Task<Result<IEnumerable<GeocodingLocation>>> ForwardGeocode(string query)
    {
        HttpClient client = new HttpClient();

        Uri uri = new Uri($"{_baseUrl}/forward?access_key={_apiKey}&query={query}");

        HttpResponseMessage response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return GeocodingLocation.Errors.InternalServerError;
        }

        string apiResponse = await response.Content.ReadAsStringAsync();
        PositionStackResponse? positionStackResponse = JsonConvert.DeserializeObject<PositionStackResponse>(apiResponse);

        if (positionStackResponse is null)
        {
            return GeocodingLocation.Errors.InternalServerError;
        }

        return Result<IEnumerable<GeocodingLocation>>.Ok(
            positionStackResponse.Data.Select(location => new GeocodingLocation(location)));
    }

    public async Task<Result<IEnumerable<string>>> ReverseGeocode(float longitude, float latitude)
    {
        HttpClient client = new HttpClient();

        Uri uri = new Uri($"{_baseUrl}/reverse?access_key={_apiKey}&query={latitude},{longitude}&limit=1");

        HttpResponseMessage response = await client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return GeocodingLocation.Errors.InternalServerError;
        }

        string apiResponse = await response.Content.ReadAsStringAsync();
        PositionStackResponse? positionStackResponse = JsonConvert.DeserializeObject<PositionStackResponse>(apiResponse);

        if (positionStackResponse is null)
        {
            return GeocodingLocation.Errors.InternalServerError;
        }

        IEnumerable<string> locations = positionStackResponse.Data.Select(location => $"{location.Locality}, {location.Region}, {location.Country}");

        return Result<IEnumerable<string>>.Ok(locations);
    }
}
