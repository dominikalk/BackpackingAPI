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
    public DateTimeOffset? DepartDate { get; set; }
    public LocationDateAccuracy LocationDateAccuracy { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    public Location(
        string name,
        float longitude,
        float latitude,
        DateTimeOffset arriveDate,
        LocationDateAccuracy locationDateAccuracy,
        Guid userId)
    {
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
        ArriveDate = arriveDate;
        LocationDateAccuracy = locationDateAccuracy;
        UserId = userId;
    }

    public Location(
        string name,
        float longitude,
        float latitude,
        DateTimeOffset arriveDate,
        DateTimeOffset departDate,
        LocationDateAccuracy locationDateAccuracy,
        Guid userId) : this(
            name,
            longitude,
            latitude,
            arriveDate,
            locationDateAccuracy,
            userId)
    {
        DepartDate = departDate;
    }
}

public enum LocationDateAccuracy
{
    Day,
    Week,
    Month
}
