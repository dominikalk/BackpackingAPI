using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>
    /// Will return the current location of the current user if it exists
    /// </summary>
    /// <returns>The current location of the current user or null</returns>
    public async Task<Result<Location?>> GetCurrentLocation()
    {
        Result<Location?> result = await _userService.GetCurrentUserId()
            .Then(GetCurrentLocation);

        return result;
    }

    /// <summary>
    /// Given the log information, will log said information to a new current location,
    /// whilst departing the old one if it existed.
    /// </summary>
    /// <param name="locationDTO">The dto for the log current location</param>
    /// <returns>The logged current location</returns>
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

    /// <summary>
    /// Will depart from the current user's current location
    /// </summary>
    /// <returns>The departed location</returns>
    public async Task<Result<Location>> DepartCurrentLocation()
    {
        return await (await GetCurrentLocation())
            .Then(GuardLocationExists)
            .Then(location => location.DepartLocation())
            .Then(SaveChanges);
    }

    /// <summary>
    /// Given an id of a visited location to update, and an update dto, will update the location
    /// and the previous + next locations if required given validation rules pass.
    /// </summary>
    /// <param name="id">The id of the visited location to update</param>
    /// <param name="locationDTO">The dto values for the update</param>
    /// <returns>The updated visited location</returns>
    public async Task<Result<Location>> UpdateVisitedLocation(Guid id, UpdateVisitedLocationDTO locationDTO)
    {
        return await ValidateId(id)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GetAdjacentVisitedLocations(id, userId))
            .Then(adjacentLocations => ValidateAdjacentVisitedLocationsForUpdate(adjacentLocations, locationDTO))
            .Then(adjacentLocations => UpdateAdjacentVisitedLocations(adjacentLocations, locationDTO))
            .Then(SaveChanges);
    }

    /// <summary>
    /// Will get the visited locations of the current user
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The visited locations of the current user</returns>
    public async Task<Result<PagedList<Location>>> GetVisitedLocations(BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> result = await _userService.GetCurrentUserId()
            .Then(userId => GetVisitedLocations(userId, pagingParameters));

        return result;
    }

    /// <summary>
    /// Given the log details, will log a planned location to the current user
    /// </summary>
    /// <param name="locationDTO">The dto for the logged planned location</param>
    /// <returns>The logged, validated, planned location</returns>
    public async Task<Result<Location>> LogPlannedLocation(LogPlannedLocationDTO locationDTO)
    {
        Result<Location> result = await _userService.GetCurrentUserId()
            .Then(userId => Result<Location>.Ok(Location.Create(locationDTO, userId)))
            .Then(ValidatePlannedLocation)
            .Then(AddLocation)
            .Then(SaveChanges);

        return result;
    }

    /// <summary>
    /// Given the id of a current user's planned location and update values, will update said
    /// location, validate it, and retuurn the updated location.
    /// </summary>
    /// <param name="id">The id of the planned location</param>
    /// <param name="locationDTO">The update dto for the planned location</param>
    /// <returns>The updated, validated planned location</returns>
    public async Task<Result<Location>> UpdatePlannedLocation(Guid id, UpdatePlannedLocationDTO locationDTO)
    {
        Result<Location> result = await GetLocationById(id)
            .Then(location => location.UpdatePlannedLocation(locationDTO))
            .Then(ValidatePlannedLocation)
            .Then(SaveChanges);

        return result;
    }

    /// <summary>
    /// Will return the future planned locations of the current user
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The future planned locations of the current user</returns>
    public async Task<Result<PagedList<Location>>> GetPlannedLocations(BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> result = await _userService.GetCurrentUserId()
            .Then(userId => GetPlannedLocations(userId, pagingParameters));

        return result;
    }

    /// <summary>
    /// Given the id of a location the current user owns, will fetch and 
    /// return it
    /// </summary>
    /// <param name="id">The id of the location</param>
    /// <returns>The fetched location</returns>
    public async Task<Result<Location>> GetLocationById(Guid id)
    {
        Result<Location> result = await ValidateId(id)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GetLocationById(id, userId))
            .Then(GuardLocationExists);

        return result;
    }

    /// <summary>
    /// Given the id of a location the current user owns, will delete it
    /// </summary>
    /// <param name="id">The id of the location to delete</param>
    /// <returns>Ok Result</returns>
    public async Task<Result> DeleteLocation(Guid id)
    {
        return await (await GetLocationById(id))
            .Then(DeleteLocation)
            .Then(SaveChanges);
    }

    /// <summary>
    /// Given a planned location will validate the location by arrive date rules, depart date rules, 
    /// and location type rules. If any fail an error will be returned
    /// </summary>
    /// <param name="location">The location to validate</param>
    /// <returns>The validated location</returns>
    private Result<Location> ValidatePlannedLocation(Location location)
    {
        Result guard = Result.Guard(() => Guard.IsBeforeOrEqual(DateTimeOffset.UtcNow, location.ArriveDate), Location.Errors.ArriveDateFuture)
            .Guard(() => Guard.IsBeforeOrEqual(location.ArriveDate, location.DepartDate), Location.Errors.ArriveBeforeDepart)
            .Guard(() => Guard.IsEqual(location.LocationType, LocationType.PlannedLocation), Location.Errors.LocationPlanned);

        if (guard.Success is false)
        {
            return guard.Error;
        }

        return location;
    }

    /// <summary>
    /// Given a user id, will return the current location of that user if it exists
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <returns>The user's current location or null</returns>
    private async Task<Result<Location?>> GetCurrentLocation(Guid userId)
    {
        return await _bPContext.Locations
           .Where(location =>
                location.DepartDate >= DateTimeOffset.UtcNow
                && location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation)
           .OrderByDescending(location => location.ArriveDate)
           .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Given a user id and paging parameters, will return the visited locations.
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The visited locations as a paged list</returns>
    private async Task<Result<PagedList<Location>>> GetVisitedLocations(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Locations
            .Where(location =>
                location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation)
            .OrderByDescending(location => location.ArriveDate)
            .ThenByDescending(location => location.DepartDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a location id and user id, will return the visited location and the previous
    /// and next adjacent ones.
    /// </summary>
    /// <param name="id">The Id of the visited location being queried for</param>
    /// <param name="userId">The Id of the user</param>
    /// <returns>The previous, middle, and next locations according to the middle location's Id</returns>
    private async Task<Result<AdjacentLocations>> GetAdjacentVisitedLocations(Guid id, Guid userId)
    {
        Result<Location> middleLocation = await GetLocationById(id, userId)
            .Then(GuardLocationExists)
            .Then(GuardLocationVisited);

        if (middleLocation.Success is false)
        {
            return middleLocation.Error;
        }

        Location? nextLocation = _bPContext.Locations
            .Where(location =>
                location.Id != middleLocation.Value.Id
                && location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation
                && location.ArriveDate >= middleLocation.Value.DepartDate)
            .OrderBy(location => location.ArriveDate)
            .ThenBy(location => location.DepartDate)
            .FirstOrDefault();

        Location? prevLocation = _bPContext.Locations
            .Where(location =>
                location.Id != middleLocation.Value.Id
                && location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation
                && location.DepartDate <= middleLocation.Value.ArriveDate)
            .OrderByDescending(location => location.DepartDate)
            .ThenByDescending(location => location.ArriveDate)
            .FirstOrDefault();

        return new AdjacentLocations(prevLocation, middleLocation.Value, nextLocation);
    }

    /// <summary>
    /// Given adjacent locations and an update visited location dto, will validate that:
    /// -   The arrive date of the previous location is before or equal to the updated location arrive date
    /// -   The depart date of the next location is after or equal to the updated location depart date
    /// -   The arrive date of the updated location is before or equal to the updated location depart date
    /// -   The depart date of the updated location is in the past or null
    /// </summary>
    /// <param name="adjacentLocations">The adjacent locations</param>
    /// <param name="locationDTO">The dto for the values of the updated visited locatio</param>
    /// <returns>The validated adjacent locations</returns>
    private Result<AdjacentLocations> ValidateAdjacentVisitedLocationsForUpdate(AdjacentLocations adjacentLocations, UpdateVisitedLocationDTO locationDTO)
    {
        Result guard = Result.Guard(() => Guard.IsBeforeOrEqual(adjacentLocations.PreviousLocation?.ArriveDate, locationDTO.ArriveDate), Location.Errors.ArriveAfterPreviousArrive)
            .Guard(() => Guard.IsBeforeOrEqual(DateOrMax(locationDTO.DepartDate), adjacentLocations.NextLocation?.DepartDate), Location.Errors.DepartBeforeNextDepart)
            .Guard(() => Guard.IsBeforeOrEqual(locationDTO.ArriveDate, locationDTO.DepartDate), Location.Errors.ArriveBeforeDepart)
            .Guard(() => Guard.IsBeforeOrEqual(locationDTO.DepartDate, DateTimeOffset.UtcNow), Location.Errors.DepartDatePast);

        if (guard.Success is false)
        {
            return guard.Error;
        }

        return adjacentLocations;
    }

    /// <summary>
    /// Given adjacent locations and an update visited location dto, which have previously been validated,
    /// will update the 3 locations as needed and return the middle location.
    /// </summary>
    /// <param name="adjacentLocations">The adjacent locations</param>
    /// <param name="locationDTO">The dto for the values of the updated visited location</param>
    /// <returns>The updated visited location</returns>
    private Result<Location> UpdateAdjacentVisitedLocations(AdjacentLocations adjacentLocations, UpdateVisitedLocationDTO locationDTO)
    {
        if (adjacentLocations.PreviousLocation?.DepartDate > locationDTO.ArriveDate)
        {
            adjacentLocations.PreviousLocation.DepartDate = locationDTO.ArriveDate;
        }

        if (adjacentLocations.NextLocation?.ArriveDate < DateOrMax(locationDTO.DepartDate))
        {
            adjacentLocations.NextLocation.ArriveDate = DateOrMax(locationDTO.DepartDate);
        }

        return adjacentLocations.Location.UpdateVisitedLocation(locationDTO);
    }

    /// <summary>
    /// Given a user id and paging parameters, will return the planned locations that depart in 
    /// the future.
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The planned future locations as a paged list</returns>
    private async Task<Result<PagedList<Location>>> GetPlannedLocations(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Locations
            .Where(location =>
                location.DepartDate >= DateTimeOffset.UtcNow
                && location.UserId == userId
                && location.LocationType == LocationType.PlannedLocation)
            .OrderBy(location => location.ArriveDate)
            .ThenBy(location => location.DepartDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a location Id and the Id of the user will return the location
    /// </summary>
    /// <param name="id">The id of the location</param>
    /// <param name="userId">The id of the user</param>
    /// <returns>The location or null</returns>
    private async Task<Result<Location?>> GetLocationById(Guid id, Guid userId)
    {
        return await _bPContext.Locations
            .Where(location => location.Id == id && location.UserId == userId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks if a Guid is valid
    /// </summary>
    /// <param name="id">The Id to check</param>
    /// <returns>The Id</returns>
    private Result<Guid> ValidateId(Guid id)
    {
        if (id == default)
        {
            return Location.Errors.InvalidId;
        }

        return id;
    }

    /// <summary>
    /// Will guard whether a location has been provided and will return an
    /// error if it hasn't
    /// </summary>
    /// <param name="location">The location to check if is null</param>
    /// <returns>The location</returns>
    private Result<Location> GuardLocationExists(Location? location)
    {
        if (location is null)
        {
            return Location.Errors.LocationNotFound;
        }

        return location;
    }

    /// <summary>
    /// Will guard whether a location is a visited location
    /// </summary>
    /// <param name="location">The location to check if is visited</param>
    /// <returns>The visited location</returns>
    private Result<Location> GuardLocationVisited(Location location)
    {
        if (location.LocationType != LocationType.VisitedLocation)
        {
            return Location.Errors.LocationVisited;
        }

        return location;
    }

    /// <summary>
    /// Given a location, will depart from that location by setting the depart
    /// date to now
    /// </summary>
    /// <param name="location">The location to depart</param>
    /// <returns>The departed location or null</returns>
    private Result<Location?> OptionalDepartLocation(Location? location)
    {
        location?.DepartLocation();
        return location;
    }

    /// <summary>
    /// Will return the date provided or the max value if not
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns>The date provided or the max value if not</returns>
    private DateTimeOffset DateOrMax(DateTimeOffset? date)
    {
        return date ?? DateTimeOffset.MaxValue;
    }

    /// <summary>
    /// Adds the location to the database context
    /// </summary>
    /// <param name="location">The location to add</param>
    /// <returns>The location</returns>
    private Result<Location> AddLocation(Location location)
    {
        _bPContext.Locations.Add(location);
        return location;
    }

    /// <summary>
    /// Deletes the location from the database context
    /// </summary>
    /// <param name="location">The location to delete</param>
    /// <returns>Ok Result</returns>
    private Result DeleteLocation(Location location)
    {
        _bPContext.Locations.Remove(location);
        return Result.Ok();
    }

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <param name="location">The location</param>
    /// <returns>The location</returns>
    private async Task<Result<Location>> SaveChanges(Location location)
    {
        await _bPContext.SaveChangesAsync();
        return location;
    }

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <returns>Ok Result</returns>
    private async Task<Result> SaveChanges()
    {
        await _bPContext.SaveChangesAsync();
        return Result.Ok();
    }
}
