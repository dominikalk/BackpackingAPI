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
        return await GetCurrentUserLocations()
            .Then(locations => Result<Location?>.Ok(locations.FirstOrDefault()));
    }

    public async Task<Result<Location>> LogCurrentLocation(LogCurrentLocationDTO locationDTO)
    {
        Result<Location?> oldLocationResult = await GetCurrentLocation()
            .Then(SetLocationDepart);

        if (!oldLocationResult.Success)
        {
            return oldLocationResult.Error;
        }

        return await _userService.GetCurrentUserId()
            .Then(userId => CreateCurrentLocation(locationDTO, userId))
            .Then(AddLocation)
            .Then(SaveChanges);
    }

    public async Task<Result<Location>> GetLocationById(Guid id)
    {
        if (id == default)
        {
            return new BPError(HttpStatusCode.BadRequest, "Invalid Id");
        }

        Location? result = await _bPContext.Locations.FirstOrDefaultAsync(location => location.Id == id);

        if (result is null)
        {
            return new BPError(HttpStatusCode.NotFound, $"Location with id: '{id}' does not exist.");
        }

        return result;
    }

    private Result<Location> CreateCurrentLocation(LogCurrentLocationDTO locationDto, Guid userId)
    {
        Location location = new Location(
            locationDto.Name,
            locationDto.Longitude,
            locationDto.Latitude,
            DateTimeOffset.Now,
            LocationDateAccuracy.Day,
            userId);

        return Result<Location>.Ok(location);
    }

    private async Task<Result<IEnumerable<Location>>> GetCurrentUserLocations()
    {
        Result<Guid> userId = _userService.GetCurrentUserId();

        if (!userId.Success)
        {
            return userId.Error;
        }

        IEnumerable<Location> locations = (await _bPContext.Users.Include(u => u.Locations)
            .FirstOrDefaultAsync(u => u.Id == userId.Value))?.Locations ?? new List<Location>();

        return Result<IEnumerable<Location>>.Ok(locations);
    }

    private Result<Location?> SetLocationDepart(Location? location)
    {
        if (location is null)
        {
            return Result<Location?>.Ok(null);
        }

        location.DepartDate = DateTimeOffset.Now;
        return Result<Location?>.Ok(location);
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
