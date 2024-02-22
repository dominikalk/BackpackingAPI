using Backpacking.API.Utils;
using System.Net;

namespace Backpacking.API.Models.Geocoding;

public class GeocodingLocation
{
    public string Label { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public string Region { get; set; } = string.Empty;

    public GeocodingLocation(PositionStackLocation location)
    {
        Label = location.Label;
        Longitude = location.Longitude;
        Latitude = location.Latitude;
        Region = location.Region;
    }

    public GeocodingLocation(GoogleLocation location)
    {
        Label = location.Formatted_Address;
        Longitude = location.Geometry.Location.Lng;
        Latitude = location.Geometry.Location.Lat;
    }

    public class Errors
    {
        public static BPError InternalServerError = new BPError(HttpStatusCode.InternalServerError, "Internal server error");
    }
}
