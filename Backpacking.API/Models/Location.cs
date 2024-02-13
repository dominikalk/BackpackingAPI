using Backpacking.API.Models.DTO;

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
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow,
            LocationType = LocationType.VisitedLocation
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

    public void DepartLocation()
    {
        DepartDate = DateTimeOffset.UtcNow;
    }
}

public enum LocationType
{
    VisitedLocation,
    PlannedLocation
}
