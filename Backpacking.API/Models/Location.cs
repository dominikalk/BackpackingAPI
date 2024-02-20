using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Utils;
using System.Net;

namespace Backpacking.API.Models;

public class Location : IBPModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public Guid UserId { get; init; }
    public BPUser? User { get; init; }
    public DateTimeOffset ArriveDate { get; set; }
    public DateTimeOffset DepartDate { get; set; } = DateTimeOffset.MaxValue;
    public LocationType LocationType { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    public Location() { }

    public static Location Create(LogCurrentLocationDTO currentLocationDto, Guid userId)
    {
        return new Location()
        {
            Name = currentLocationDto.Name,
            Longitude = currentLocationDto.Longitude,
            Latitude = currentLocationDto.Latitude,
            ArriveDate = DateTimeOffset.UtcNow,
            LocationType = LocationType.VisitedLocation,
            UserId = userId,
        };
    }

    public static Location Create(LogPlannedLocationDTO plannedLocationDto, Guid userId)
    {
        return new Location()
        {
            Name = plannedLocationDto.Name,
            Longitude = plannedLocationDto.Longitude,
            Latitude = plannedLocationDto.Latitude,
            ArriveDate = plannedLocationDto.ArriveDate,
            DepartDate = plannedLocationDto.DepartDate ?? DateTimeOffset.MaxValue,
            LocationType = LocationType.PlannedLocation,
            UserId = userId,
        };
    }

    public Result<Location> DepartLocation()
    {
        DepartDate = DateTimeOffset.UtcNow;
        return this;
    }

    public Result<Location> UpdateVisitedLocation(UpdateVisitedLocationDTO dto)
    {
        if (LocationType == LocationType.PlannedLocation)
        {
            return Errors.LocationVisited;
        }

        Name = dto.Name;
        Longitude = dto.Longitude;
        Latitude = dto.Latitude;
        ArriveDate = dto.ArriveDate;
        DepartDate = dto.DepartDate ?? DateTimeOffset.MaxValue;

        return this;
    }


    public Result<Location> UpdatePlannedLocation(UpdatePlannedLocationDTO dto)
    {
        if (LocationType == LocationType.VisitedLocation)
        {
            return Errors.LocationPlanned;
        }

        Name = dto.Name;
        Longitude = dto.Longitude;
        Latitude = dto.Latitude;
        ArriveDate = dto.ArriveDate;
        DepartDate = dto.DepartDate ?? DateTimeOffset.MaxValue;

        return this;
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
    }
}

public class AdjacentLocations
{
    public Location? PreviousLocation { get; set; }
    public Location Location { get; set; }
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
