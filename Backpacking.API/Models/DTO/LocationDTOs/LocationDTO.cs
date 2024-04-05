namespace Backpacking.API.Models.DTO.LocationDTOs;

public class LocationDTO
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public DateTimeOffset ArriveDate { get; set; }
    public DateTimeOffset? DepartDate { get; set; }

    public LocationDTO(Location location)
    {
        Id = location.Id;
        Name = location.Name;
        Longitude = Location.RoundCoordinatePrecision(location.Longitude);
        Latitude = Location.RoundCoordinatePrecision(location.Latitude);
        ArriveDate = location.ArriveDate;
        DepartDate = location.DepartDate == DateTimeOffset.MaxValue ? null : location.DepartDate;
    }
}
