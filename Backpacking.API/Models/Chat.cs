namespace Backpacking.API.Models;

public class Chat : IBPModel
{
    /// <summary>
    /// The id of the chat
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The users that are part of the chat
    /// </summary>
    public IEnumerable<BPUser> Users { get; init; } = new List<BPUser>();

    /// <summary>
    /// The messages that are part of the chat
    /// </summary>
    public IEnumerable<ChatMessage> Messages { get; init; } = new List<ChatMessage>();

    /// <summary>
    /// The date the chat was created on
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The date the chat was last modified on
    /// </summary>
    public DateTimeOffset LastModifiedDate { get; set; }
}
