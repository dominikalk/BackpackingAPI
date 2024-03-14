namespace Backpacking.API.Models.DTO.ChatDTOs;

public class PrivateChatDetailsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<PrivateChatDetailsDTOUser> Users { get; set; } = new List<PrivateChatDetailsDTOUser>();
    public int UnreadCount { get; set; }
    public bool IsFriend { get; set; }


    public PrivateChatDetailsDTO(Chat chat, Guid currentUserId)
    {
        Id = chat.Id;
        Name = String.Join(", ", chat.Users
            .Where(user => user.Id != currentUserId)
            .Select(user => user.FullName));
        Users = chat.Users
            .Where(user => user.Id != currentUserId)
            .Select(user => new PrivateChatDetailsDTOUser(user));
        UnreadCount = Chat.GetChatUnreadCount(chat, currentUserId);

        BPUser user = chat.Users.First(user => user.Id != currentUserId);

        IsFriend = user.SentUserRelations
                .Any(relation =>
                    relation.SentToId == currentUserId
                    && relation.RelationType == UserRelationType.Friend)
            || user.ReceivedUserRelations
                .Any(relation =>
                    relation.SentById == currentUserId
                    && relation.RelationType == UserRelationType.Friend);
    }
}

public class PrivateChatDetailsDTOUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public PrivateChatDetailsDTOUser(BPUser user)
    {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.UserName ?? string.Empty;
    }
}
