using Backpacking.API.Utils;
using System.Net;

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
    public ICollection<BPUser> Users { get; init; } = new List<BPUser>();

    /// <summary>
    /// The messages that are part of the chat
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// The last read date of the users
    /// </summary>
    public ICollection<ChatUserRead> UserLastReadDate { get; set; } = new List<ChatUserRead>();

    /// <summary>
    /// The date the chat was created on
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The date the chat was last modified on
    /// </summary>
    public DateTimeOffset LastModifiedDate { get; set; }

    /// <summary>
    /// Given a user id, friend id, and message content, will create a new
    /// private chat between the users with the content as the first message
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="friendId">The friend's id</param>
    /// <param name="content">The message content</param>
    /// <returns>The chat</returns>
    public static Result<Chat> CreatePrivateChat(BPUser user, BPUser friend, string content)
    {
        Chat chat = new Chat();

        chat.Messages.Add(new ChatMessage()
        {
            UserId = user.Id,
            Content = content,
        });
        chat.Users.Add(user);
        chat.Users.Add(friend);

        return chat;
    }

    /// <summary>
    /// Given a chat and the user's id, will return the chat's 
    /// unread message count for the user.
    /// </summary>
    /// <param name="chat">The chat</param>
    /// <param name="userId">The user;s id</param>
    /// <returns>The chat unread messages count for the user</returns>
    public static int GetChatUnreadCount(Chat chat, Guid userId)
    {
        ChatUserRead? chatUserRead = chat.UserLastReadDate
            .SingleOrDefault(chatUserRead => chatUserRead.UserId == userId);

        int unreadCount = chat.Messages
            .OrderByDescending(message => message.CreatedDate)
            .TakeWhile(message =>
                message.UserId != userId
                && message.CreatedDate > (chatUserRead?.LastReadDate ?? DateTimeOffset.MinValue))
            .Count();

        return unreadCount;
    }

    public class Errors
    {
        public static BPError InvalidId = new BPError(HttpStatusCode.BadRequest, "Invalid Id.");
        public static BPError ChatNotFound = new BPError(HttpStatusCode.NotFound, "Chat not found.");
        public static BPError UsersNotFriends = new BPError(HttpStatusCode.BadRequest, "Users are not friends or friends doesn't exist.");
        public static BPError ChatExists = new BPError(HttpStatusCode.BadRequest, "A private chat with the user already exists.");
        public static BPError RelationIdNotUserId = new BPError(HttpStatusCode.BadRequest, "Relation Id cannot be user's Id.");
    }
}