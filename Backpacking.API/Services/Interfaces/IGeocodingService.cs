using Backpacking.API.Models.Geocoding;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IGeocodingService
{
    Task<Result<IEnumerable<GeocodingLocation>>> ForwardGeocode(string query);
    Task<Result<IEnumerable<string>>> ReverseGeocode(float longitude, float latitude);
}
