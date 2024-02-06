namespace Backpacking.API.Models;

public class Location : IModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public Guid UserId { get; init; }
    public User? User { get; init; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    public Location(string Name, float Longitude, float Latitude, Guid userId)
    {
        this.Name = Name;
        this.Longitude = Longitude;
        this.Latitude = Latitude;
        UserId = userId;
    }
}
