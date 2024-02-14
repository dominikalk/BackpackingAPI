using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backpacking.API.Services;

public class LocationService : ILocationService
{
    private readonly IBPContext _bPContext;
    private readonly IUserService _userService;

    public LocationService(
        IBPContext bPContext,
        IUserService userService)
    {
        _bPContext = bPContext;
        _userService = userService;
    }

    public async Task<Result<Location?>> GetCurrentLocation()
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (currentUserId.Success is false)
        {
            return currentUserId.Error;
        }

        return await GetUserLocationsQueryable(currentUserId.Value)
            .Where(location =>
                location.LocationType == LocationType.VisitedLocation &&
                location.DepartDate > DateTimeOffset.UtcNow)
            .OrderByDescending(location => location.ArriveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO)
    {
        Result<Location?> oldLocation = await GetCurrentLocation();

        if (!oldLocation.Success)
        {
            return oldLocation.Error;
        }

        oldLocation.Value?.DepartLocation();

        return await _userService.GetCurrentUserId()
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(AddLocation)
            .Then(SaveChanges);
    }

    public async Task<Result<Location>> LogPlannedLocation(LogPlannedLocationDTO locationDTO)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(AddLocation)
            .Then(SaveChanges);
    }

    public async Task<Result<IEnumerable<Location>>> GetPlannedLocations()
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (currentUserId.Success is false)
        {
            return currentUserId.Error;
        }

        return await GetUserLocationsQueryable(currentUserId.Value)
            .Where(location =>
                location.LocationType == LocationType.PlannedLocation
                && location.ArriveDate > DateTimeOffset.UtcNow)
            .OrderBy(location => location.ArriveDate)
            .ToListAsync();
    }

    public async Task<Result<Location>> GetLocationById(Guid id)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        Result guard = Result.Guard(() => Guard.IsNotEqual(id, Guid.Empty), new BPError(HttpStatusCode.BadRequest, "Invalid Id"))
            .Guard(() => Guard.IsEqual(currentUserId.Success, true), currentUserId.Error);

        if (guard.Success is false)
        {
            return guard.Error;
        }

        Location? location = await GetUserLocationsQueryable(currentUserId.Value).FirstOrDefaultAsync(location => location.Id == id);

        if (location is null)
        {
            return new BPError(HttpStatusCode.NotFound, $"Location with id: '{id}' does not exist.");
        }

        return location;
    }

    //private Result ValidateLogLocationDto(LogPlannedLocationDTO logLocation)
    //{
    //    return Result.Guard(() => Guard.IsBefore(DateOnly.FromDateTime(DateTimeOffset.UtcNow), logLocation.ArriveDate), new BPError(HttpStatusCode.BadRequest, "Name is required"));
    //}

    private IQueryable<Location> GetUserLocationsQueryable(Guid userId)
    {
        return _bPContext.Locations.Where(location => location.UserId == userId);
    }

    private Result<Location> AddLocation(Location location)
    {
        _bPContext.Locations.Add(location);
        return Result<Location>.Ok(location);
    }

    private async Task<Result<Location>> SaveChanges(Location location)
    {
        await _bPContext.SaveChangesAsync();
        return Result<Location>.Ok(location);
    }
}
