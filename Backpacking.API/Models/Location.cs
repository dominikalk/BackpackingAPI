using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Utils;
using System.Net;

namespace Backpacking.API.Models;

public class Location : IBPModel
{
    /// <summary>
    /// The id of the location
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The name of the location
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The longitude of the location
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// The latitude of the location
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    /// The id of the user the location belongs to
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The user the location belongs to
    /// </summary>
    public BPUser? User { get; init; }

    /// <summary>
    /// The date the location was / is planned to be arrived at
    /// </summary>
    public DateTimeOffset ArriveDate { get; set; }

    /// <summary>
    /// The date the location was / is planned to be departed at
    /// </summary>
    public DateTimeOffset DepartDate { get; set; } = DateTimeOffset.MaxValue;

    /// <summary>
    /// The type of location (visited or planned)
    /// </summary>
    public LocationType LocationType { get; set; }

    /// <summary>
    /// The date the location was created on
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The date the location was last modified on
    /// </summary>
    public DateTimeOffset LastModifiedDate { get; set; }

    public Location() { }

    /// <summary>
    /// Given a current location dto and a user id, will return a new current location
    /// based on their values
    /// </summary>
    /// <param name="currentLocationDto">The values for the current location</param>
    /// <param name="userId">The user's id</param>
    /// <returns>The new current location</returns>
    public static Location Create(LogCurrentLocationDTO currentLocationDto, Guid userId)
    {
        return new Location()
        {
            Name = currentLocationDto.Name,
            Longitude = RoundCoordinatePrecision(currentLocationDto.Longitude),
            Latitude = RoundCoordinatePrecision(currentLocationDto.Latitude),
            ArriveDate = DateTimeOffset.UtcNow,
            LocationType = LocationType.VisitedLocation,
            UserId = userId,
        };
    }

    /// <summary>
    /// Given a planned location dto and a user id, will return a new planned location
    /// based on their values
    /// </summary>
    /// <param name="plannedLocationDto">The values for the planned location</param>
    /// <param name="userId">The user's id</param>
    /// <returns>The new planned location</returns>
    public static Location Create(LogPlannedLocationDTO plannedLocationDto, Guid userId)
    {
        return new Location()
        {
            Name = plannedLocationDto.Name,
            Longitude = RoundCoordinatePrecision(plannedLocationDto.Longitude),
            Latitude = RoundCoordinatePrecision(plannedLocationDto.Latitude),
            ArriveDate = plannedLocationDto.ArriveDate,
            DepartDate = plannedLocationDto.DepartDate ?? DateTimeOffset.MaxValue,
            LocationType = LocationType.PlannedLocation,
            UserId = userId,
        };
    }

    /// <summary>
    /// Will depart from the location
    /// </summary>
    /// <returns>The departed location</returns>
    public Result<Location> DepartLocation()
    {
        DepartDate = DateTimeOffset.UtcNow;
        return this;
    }

    /// <summary>
    /// Will update the visited location based on the dto properties
    /// </summary>
    /// <param name="dto">The properties to update the location with</param>
    /// <returns>The updated location</returns>
    public Result<Location> UpdateVisitedLocation(UpdateVisitedLocationDTO dto)
    {
        if (LocationType != LocationType.VisitedLocation)
        {
            return Errors.LocationVisited;
        }

        Name = dto.Name;
        Longitude = RoundCoordinatePrecision(dto.Longitude);
        Latitude = RoundCoordinatePrecision(dto.Latitude);
        ArriveDate = dto.ArriveDate;
        DepartDate = dto.DepartDate ?? DateTimeOffset.MaxValue;

        return this;
    }

    /// <summary>
    /// Will update the planned location based on the dto properties
    /// </summary>
    /// <param name="dto">The properties to update the location with</param>
    /// <returns>The updated location</returns>
    public Result<Location> UpdatePlannedLocation(UpdatePlannedLocationDTO dto)
    {
        if (LocationType != LocationType.PlannedLocation)
        {
            return Errors.LocationPlanned;
        }

        Name = dto.Name;
        Longitude = RoundCoordinatePrecision(dto.Longitude);
        Latitude = RoundCoordinatePrecision(dto.Latitude);
        ArriveDate = dto.ArriveDate;
        DepartDate = dto.DepartDate ?? DateTimeOffset.MaxValue;

        return this;
    }

    /// <summary>
    /// Given a coordinate, will round it to 1.11km precision
    /// </summary>
    /// <param name="coordinate">The coordinate to round</param>
    /// <returns>The rounded coordinate</returns>
    public static float RoundCoordinatePrecision(float coordinate)
    {
        return (float)Math.Round(coordinate, 2);
    }

    public class Errors
    {
        public static BPError InvalidId = new BPError(HttpStatusCode.BadRequest, "Invalid Id.");
        public static BPError ArriveDateFuture = new BPError(HttpStatusCode.BadRequest, "Arrive date must be in the future.");
        public static BPError ArriveBeforeDepart = new BPError(HttpStatusCode.BadRequest, "Arrive date must be before or equal to depart date.");
        public static BPError LocationPlanned = new BPError(HttpStatusCode.BadRequest, "Location cannot be visited location.");
        public static BPError LocationVisited = new BPError(HttpStatusCode.BadRequest, "Location cannot be planned location.");
        public static BPError LocationNotFound = new BPError(HttpStatusCode.NotFound, "Location not found.");

        //Errors Relating To Updating Visited Locations
        public static BPError ArriveAfterPreviousArrive = new BPError(HttpStatusCode.BadRequest, "Arrive date must be after or equal to previous location arrive date.");
        public static BPError DepartBeforeNextDepart = new BPError(HttpStatusCode.BadRequest, "Depart date must be before or equal to next location depart date.");
        public static BPError DepartDatePast = new BPError(HttpStatusCode.BadRequest, "Depart date must be in the past.");
        public static BPError ArriveDatePast = new BPError(HttpStatusCode.BadRequest, "Arrive date must be in the past.");
    }
}

public class AdjacentLocations
{
    /// <summary>
    /// The previous location to the location
    /// </summary>
    public Location? PreviousLocation { get; set; }

    /// <summary>
    /// The location
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    /// The next location to the location
    /// </summary>
    public Location? NextLocation { get; set; }

    public AdjacentLocations(Location? previousLocation, Location location, Location? nextLocation)
    {
        PreviousLocation = previousLocation;
        Location = location;
        NextLocation = nextLocation;
    }
}

public enum LocationType
{
    VisitedLocation,
    PlannedLocation
}
