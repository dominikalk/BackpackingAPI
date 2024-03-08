namespace Backpacking.API.Models;

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
}
