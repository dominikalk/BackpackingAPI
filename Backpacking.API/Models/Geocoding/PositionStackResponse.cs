namespace Backpacking.API.Models.Geocoding;

public class PositionStackResponse
{
    public IEnumerable<PositionStackLocation> Data { get; set; } = new List<PositionStackLocation>();
}

public class PositionStackLocation
{
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Locality { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
