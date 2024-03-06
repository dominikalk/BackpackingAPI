namespace Backpacking.API.Models.DTO.NetworkDTOs;

public class FriendRequestDTO
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTimeOffset RequestedDate { get; set; }

    public FriendRequestDTO(UserRelation relation)
    {
        Id = relation.SentById;
        UserName = relation.SentBy.UserName ?? string.Empty;
        FullName = relation.SentBy.FullName;
        RequestedDate = relation.CreatedDate;
    }
}
