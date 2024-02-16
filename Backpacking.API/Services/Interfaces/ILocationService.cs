using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface ILocationService
{
    Task<Result<Location?>> GetCurrentLocation();
    Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO);
    Task<Result<Location>> DepartCurrentLocation();
    Task<Result<Location>> LogPlannedLocation(LogPlannedLocationDTO locationDTO);
    Task<Result<Location>> UpdatePlannedLocation(Guid id, UpdatePlannedLocationDTO locationDTO);
    Task<Result<PagedList<Location>>> GetVisitedLocations(BPPagingParameters pagingParameters);
    Task<Result<PagedList<Location>>> GetPlannedLocations(BPPagingParameters pagingParameters);
    Task<Result<Location>> GetLocationById(Guid id);
    Task<Result> DeleteLocationById(Guid id);
}
