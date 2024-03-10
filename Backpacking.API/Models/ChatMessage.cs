using Backpacking.API.Utils;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backpacking.API.Models;

[Table("ChatMessage")]
public class ChatMessage : IBPModel
{
    /// <summary>
    /// The id of the chat message
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user that created the message
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The user that created the message
    /// </summary>
    public BPUser? User { get; init; }

    /// <summary>
    /// The id of the chat the message belongs to
    /// </summary>
    public Guid ChatId { get; init; }

    /// <summary>
    /// The chat the message belongs to
    /// </summary>
    public Chat? Chat { get; init; }

    /// <summary>
    /// The content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The date the chat message was created on
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The date the chat message was last modified on
    /// </summary>
    public DateTimeOffset LastModifiedDate { get; set; }

    /// <summary>
    /// Given the chat id, user id, and message content will
    /// return the chat message with those values.
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <param name="userId">The user's id</param>
    /// <param name="content">The message content</param>
    /// <returns>The chat message</returns>
    public static Result<ChatMessage> Create(Guid chatId, Guid userId, string content)
    {
        ChatMessage chatMessage = new ChatMessage()
        {
            UserId = userId,
            ChatId = chatId,
            Content = content
        };

        return chatMessage;
    }
}
