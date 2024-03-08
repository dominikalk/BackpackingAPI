namespace Backpacking.API.Models.DTO.ChatDTOs;

public class ChatDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<ChatDTOUser> Users { get; set; } = new List<ChatDTOUser>();

    public ChatDTO(Chat chat, Guid currentUserId)
    {
        Id = chat.Id;
        Name = String.Join(
            ", ",
            chat.Users
                .Where(user => user.Id != currentUserId)
                .Select(user => user.FullName));
        Users = chat.Users
            .Where(user => user.Id != currentUserId)
            .Select(user => new ChatDTOUser(user));
    }
}

public class ChatDTOUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public ChatDTOUser(BPUser user)
    {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.UserName ?? string.Empty;
    }
}
