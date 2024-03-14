namespace Backpacking.API.Models;

public class ChatUserRead
{
    /// <summary>
    /// The Id of the chat
    /// </summary>
    public Guid ChatId { get; init; }

    /// <summary>
    /// The Id of the user
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The date the user last read the chat
    /// </summary>
    public DateTimeOffset LastReadDate { get; set; }
}
