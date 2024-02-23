namespace Backpacking.API.Models.DTO.FriendDTOs;

public class FriendRequestDTO
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTimeOffset RequestedDate { get; set; }

    public FriendRequestDTO(UserRelation relation)
    {
        UserId = relation.SentById;
        UserName = relation.SentBy.UserName ?? string.Empty;
        UserFullName = relation.SentBy.FullName;
        RequestedDate = relation.CreatedDate;
    }
}
