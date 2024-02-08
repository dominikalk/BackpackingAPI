namespace Backpacking.API.Models;

public class Location : IBPModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public Guid UserId { get; init; }
    public BPUser? User { get; init; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    public Location(string name, float longitude, float latitude, Guid userId)
    {
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
        UserId = userId;
    }
}
