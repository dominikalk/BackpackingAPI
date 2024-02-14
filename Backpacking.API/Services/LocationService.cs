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
        Result<Location?> result = await _userService.GetCurrentUserId()
            .Then(GetCurrentLocation);

        return result;
    }

    public async Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO)
    {
        Result<Location> result = await (await GetCurrentLocation())
            .Then(DepartLocation)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(AddLocation)
            .Then(SaveChanges);

        return result;
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
        Result guard = Result.Guard(() => Guard.IsBefore(DateTimeOffset.UtcNow, location.ArriveDate), new BPError(HttpStatusCode.BadRequest, "Arrive date must be in the future."))
            .Guard(() => Guard.IsBefore(location.ArriveDate, location.DepartDate), new BPError(HttpStatusCode.BadRequest, "Arrive date must be before depart date."));

        if (guard.Success is false)
        {
            return guard.Error;
        }

        return location;
    }

    private async Task<Result<Location?>> GetCurrentLocation(Guid userId)
    {
        return await _bPContext.VisitedLocations
           .Where(location => location.DepartDate > DateTimeOffset.UtcNow && location.UserId == userId)
           .OrderByDescending(location => location.ArriveDate)
           .FirstOrDefaultAsync();
    }

    private async Task<Result<IEnumerable<Location>>> GetPlannedLocations(Guid userId)
    {
        return await _bPContext.PlannedLocations
            .Where(location => location.ArriveDate > DateTimeOffset.UtcNow && location.UserId == userId)
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
            return new BPError(HttpStatusCode.BadRequest, "Invalid Id");
        }

        return id;
    }

    private Result<Location> GuardLocation(Location? location, Guid locationId)
    {
        if (location is null)
        {
            return new BPError(HttpStatusCode.NotFound, $"Location with id: '{locationId}' does not exist.");
        }

        return location;
    }

    private Result<Location?> DepartLocation(Location? location)
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
