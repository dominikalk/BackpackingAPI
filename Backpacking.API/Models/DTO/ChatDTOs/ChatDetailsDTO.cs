namespace Backpacking.API.Models.DTO.ChatDTOs;

public class ChatDetailsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<ChatDetailsDTOUser> Users { get; set; } = new List<ChatDetailsDTOUser>();

    public ChatDetailsDTO(Chat chat, Guid currentUserId)
    {
        Id = chat.Id;
        Name = String.Join(", ", chat.Users
            .Where(user => user.Id != currentUserId)
            .Select(user => user.FullName));
        Users = chat.Users
            .Where(user => user.Id != currentUserId)
            .Select(user => new ChatDetailsDTOUser(user));
    }

    // TODO: Add Unread Count
}

public class ChatDetailsDTOUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public ChatDetailsDTOUser(BPUser user)
    {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.UserName ?? string.Empty;
    }
}
