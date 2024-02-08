using Backpacking.API.DbContexts;
using Backpacking.API.Models;
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
        Result<Location?> result = await _userService.GetCurrentUser()
            .Then<BPUser, Location?>(currentUser => currentUser.CurrentLocation);

        return result;

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
}
