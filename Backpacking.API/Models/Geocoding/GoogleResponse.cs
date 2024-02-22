namespace Backpacking.API.Models.Geocoding;

public class GoogleResponse
{
    public IEnumerable<GoogleLocation> Results = new List<GoogleLocation>();
}

public class GoogleLocation
{
    public string Formatted_Address { get; set; } = string.Empty;
    public GoogleGeometry Geometry { get; set; } = new GoogleGeometry();
}

public class GoogleGeometry
{
    public GoogleGeometyLocation Location { get; set; } = new GoogleGeometyLocation();
}

public class GoogleGeometyLocation
{
    public float Lng { get; set; }
    public float Lat { get; set; }
}
