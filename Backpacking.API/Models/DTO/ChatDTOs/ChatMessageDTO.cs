namespace Backpacking.API.Models.DTO.ChatDTOs;

public class ChatMessageDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool OwnMessage { get; set; }
    public bool IsEdited { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public ChatMessageDTO(ChatMessage message, Guid currentUserId)
    {
        Id = message.Id;
        Content = message.Content;
        OwnMessage = message.UserId == currentUserId;
        IsEdited = message.CreatedDate != message.LastModifiedDate;
        CreatedDate = message.CreatedDate;
    }
}
