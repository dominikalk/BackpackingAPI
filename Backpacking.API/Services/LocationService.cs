using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO.LocationDTOs;
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
        Result<Location?> result = await _userService.GetCurrentUserId()
            .Then(GetCurrentLocation);

        return result;
    }

    public async Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO)
    {
        Result<Location> result = await (await GetCurrentLocation())
            .Then(OptionalDepartLocation)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(AddLocation)
            .Then(SaveChanges);

        return result;
    }

    public async Task<Result<Location>> DepartCurrentLocation()
    {
        return await (await GetCurrentLocation())
            .Then(GuardLocation)
            .Then(DepartLocation)
            .Then(SaveChanges);
    }

    public async Task<Result<Location>> LogPlannedLocation(LogPlannedLocationDTO locationDTO)
    {
        Result<Location> result = await _userService.GetCurrentUserId()
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(ValidatePlannedLocation)
            .Then(AddLocation)
            .Then(SaveChanges);

        return result;
    }

    public async Task<Result<IEnumerable<Location>>> GetPlannedLocations()
    {
        Result<IEnumerable<Location>> result = await _userService.GetCurrentUserId()
            .Then(GetPlannedLocations);

        return result;
    }

    public async Task<Result<Location>> GetLocationById(Guid id)
    {
        Result<Location> result = await ValidateId(id)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GetLocationById(id, userId))
            .Then(location => GuardLocation(location, id));

        return result;
    }

    private Result<Location> ValidatePlannedLocation(Location location)
    {
        Result guard = Result.Guard(() => Guard.IsBefore(DateTimeOffset.UtcNow, location.ArriveDate), Location.Errors.ArriveDateFuture)
            .Guard(() => Guard.IsBefore(location.ArriveDate, location.DepartDate), Location.Errors.ArriveBeforeDepart);

        if (guard.Success is false)
        {
            return guard.Error;
        }

        return location;
    }

    private async Task<Result<Location?>> GetCurrentLocation(Guid userId)
    {
        return await _bPContext.Locations
           .Where(location =>
                location.DepartDate > DateTimeOffset.UtcNow
                && location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation)
           .OrderByDescending(location => location.ArriveDate)
           .FirstOrDefaultAsync();
    }

    private async Task<Result<IEnumerable<Location>>> GetPlannedLocations(Guid userId)
    {
        return await _bPContext.Locations
            .Where(location =>
                location.DepartDate > DateTimeOffset.UtcNow
                && location.UserId == userId
                && location.LocationType == LocationType.PlannedLocation)
            .OrderBy(location => location.ArriveDate)
            .ToListAsync();
    }

    private async Task<Result<Location?>> GetLocationById(Guid id, Guid userId)
    {
        return await _bPContext.Locations
            .Where(location => location.Id == id && location.UserId == userId)
            .FirstOrDefaultAsync();
    }

    private Result<Guid> ValidateId(Guid id)
    {
        if (id == default)
        {
            return Location.Errors.InvalidId;
        }

        return id;
    }

    private Result<Location> GuardLocation(Location? location)
    {
        if (location is null)
        {
            return Location.Errors.LocationNotFound;
        }

        return location;
    }

    private Result<Location> GuardLocation(Location? location, Guid locationId)
    {
        if (location is null)
        {
            return new BPError(HttpStatusCode.NotFound, $"Location with id: '{locationId}' does not exist.");
        }

        return location;
    }

    private Result<Location> DepartLocation(Location location)
    {
        location.DepartLocation();
        return location;
    }

    private Result<Location?> OptionalDepartLocation(Location? location)
    {
        location?.DepartLocation();
        return location;
    }

    private Result<Location> AddLocation(Location location)
    {
        _bPContext.Locations.Add(location);
        return location;
    }

    private async Task<Result<Location>> SaveChanges(Location location)
    {
        await _bPContext.SaveChangesAsync();
        return location;
    }
}
