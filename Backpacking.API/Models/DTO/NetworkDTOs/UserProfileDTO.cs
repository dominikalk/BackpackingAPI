namespace Backpacking.API.Models.DTO.NetworkDTOs;

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTimeOffset JoinedDate { get; set; }
    public string Relation { get; set; }

    public UserProfileDTO(BPUser user, Guid currentUserId)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        UserName = user.UserName ?? string.Empty;
        Bio = user.Bio;
        JoinedDate = user.JoinedDate;
        Relation = GetUserRelation(user, currentUserId);
    }

    private string GetUserRelation(BPUser user, Guid currentUserId)
    {
        UserRelation? sentRelation = user.SentUserRelations
            .Where(relation => relation.SentToId == currentUserId)
            .FirstOrDefault();

        UserRelation? receivedRelation = user.ReceivedUserRelations
            .Where(relation => relation.SentById == currentUserId)
            .FirstOrDefault();

        if (sentRelation?.RelationType == UserRelationType.Friend
            || receivedRelation?.RelationType == UserRelationType.Friend)
        {
            return "friend";
        }

        if (sentRelation?.RelationType == UserRelationType.Pending)
        {
            return "requestReceived";
        }

        if (receivedRelation?.RelationType == UserRelationType.Pending)
        {
            return "requestSent";
        }

        return "none";
    }
}