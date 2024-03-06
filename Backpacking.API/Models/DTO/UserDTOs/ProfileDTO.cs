namespace Backpacking.API.Models.DTO.UserDTOs;

public class ProfileDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTimeOffset JoinedDate { get; set; }

    public ProfileDTO(BPUser user)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        UserName = user.UserName ?? string.Empty;
        Bio = user.Bio;
        JoinedDate = user.JoinedDate;
    }
}
