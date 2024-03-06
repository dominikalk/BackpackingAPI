namespace Backpacking.API.Models.DTO.NetworkDTOs;

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTimeOffset JoinedDate { get; set; }
    public bool IsFriend { get; set; }

    public UserProfileDTO(BPUser user, Guid currentUserId)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        UserName = user.UserName ?? string.Empty;
        Bio = user.Bio;
        JoinedDate = user.JoinedDate;
        IsFriend =
            user.SentUserRelations.Any(relation => relation.SentToId == currentUserId)
            || user.ReceivedUserRelations.Any(relation => relation.SentById == currentUserId);
    }
}
