namespace Backpacking.API.Models.DTO.FriendDTOs;

public class FriendLocationDTO
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public DateTimeOffset ArriveDate { get; set; }
    public DateTimeOffset? DepartDate { get; set; }
    public FriendLocationDTOUser? User { get; set; }

    public FriendLocationDTO(Location location)
    {
        Id = location.Id;
        Name = location.Name;
        Longitude = location.Longitude;
        Latitude = location.Latitude;
        ArriveDate = location.ArriveDate;
        DepartDate = location.DepartDate == DateTimeOffset.MaxValue ? null : location.DepartDate;
        if (location.User != null)
        {
            User = new FriendLocationDTOUser(location.User);
        }
    }
}

public class FriendLocationDTOUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public FriendLocationDTOUser(BPUser user)
    {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.UserName ?? string.Empty;
    }
}
