using Backpacking.API.Models;
using Backpacking.API.Models.DTO;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface ILocationService
{
    Task<Result<Location?>> GetCurrentLocation();
    Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO);
    Task<Result<Location>> LogPlannedLocation(LogPlannedLocationDTO locationDTO);
    Task<Result<IEnumerable<Location>>> GetPlannedLocations();
    Task<Result<Location>> GetLocationById(Guid id);
}
