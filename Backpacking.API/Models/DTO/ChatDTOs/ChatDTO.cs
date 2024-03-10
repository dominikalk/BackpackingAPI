namespace Backpacking.API.Models.DTO.ChatDTOs;

public class ChatDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnreadCount { get; set; }

    public ChatDTO(Chat chat, Guid currentUserId)
    {
        Id = chat.Id;
        Name = String.Join(
            ", ",
            chat.Users
                .Where(user => user.Id != currentUserId)
                .Select(user => user.FullName));
        UnreadCount = Chat.GetChatUnreadCount(chat, currentUserId);
    }
}
