using Backpacking.API.DbContexts;
using Backpacking.API.Hubs;
using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.ChatDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Services;

public class ChatService : IChatService
{
    private readonly IBPContext _bPContext;
    private readonly IUserService _userService;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public ChatService(
        IBPContext bPContext,
        IUserService userService,
        IHubContext<ChatHub> chatHubContext)
    {
        _bPContext = bPContext;
        _userService = userService;
        _chatHubContext = chatHubContext;
    }

    /// <summary>
    /// Returns the current user's chats
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The current user's chats</returns>
    public async Task<Result<PagedList<Chat>>> GetChats(BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => GetChats(userId, pagingParameters));
    }

    /// <summary>
    /// Given a chat id, if the current user is a participant, will 
    /// return the chat.
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <returns>The chat</returns>
    public async Task<Result<Chat>> GetChatById(Guid chatId)
    {
        return await ValidateId(chatId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GetUserChatById(userId, chatId));
    }

    /// <summary>
    /// Given a chat id, if the current user is a participant, will
    /// return the paged chat messages
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The paged chat messages</returns>
    public async Task<Result<PagedList<ChatMessage>>> GetChatMessages(Guid chatId, BPPagingParameters pagingParameters)
    {
        return await ValidateId(chatId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => ValidateUserInChat(userId, chatId))
            .Then(_ => GetValidatedChatMessages(chatId, pagingParameters));
    }

    /// <summary>
    /// Given a friend's id and the content of the first message, will
    /// create a private chat with the message given the chat doesn't already
    /// exist and the users are friends.
    /// </summary>
    /// <param name="createPrivateChatDTO">Then user's id and the message content</param>
    /// <returns>The private chat</returns>
    public async Task<Result<Chat>> CreatePrivateChat(CreatePrivateChatDTO createPrivateChatDTO)
    {
        Result<Chat> chat = await ValidateId(createPrivateChatDTO.UserId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, createPrivateChatDTO.UserId))
            .Then(userId => ValidatePrivateChatCreatable(userId, createPrivateChatDTO.UserId))
            .Then(chatUsers => Chat.CreatePrivateChat(chatUsers.User, chatUsers.Friend, createPrivateChatDTO.Content))
            .Then(AddChat)
            .Then(SaveChanges);

        // Send SignalR Update To Other User
        if (chat.Success)
        {
            await _chatHubContext.Clients.User(createPrivateChatDTO.UserId.ToString()).SendAsync(
                "ReceiveNewChat",
                new { ChatId = chat.Value.Id }
            );
        }

        return chat;
    }

    /// <summary>
    /// Given a chat id and the message content, will add the message to
    /// the chat if the current user is a participant
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <param name="createChatMessageDTO">The message content</param>
    /// <returns>The chat message</returns>
    public async Task<Result<ChatMessage>> CreateChatMessage(Guid chatId, CreateChatMessageDTO createChatMessageDTO)
    {
        Result<ChatMessage> message = await ValidateId(chatId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => ValidateChatMessageCreatable(userId, chatId))
            .Then(userId => ChatMessage.Create(chatId, userId, createChatMessageDTO.Content))
            .Then(AddChatMessage)
            .Then(SaveChanges);

        // Send SignalR Update To Other Chat Users
        if (message.Success)
        {
            Chat chat = await _bPContext.Chats
                .Include(chat => chat.Users)
                .FirstAsync(chat => chat.Id == chatId);

            IReadOnlyList<string> userIds = chat.Users
                .Where(user => user.Id != message.Value.UserId)
                .Select(user => user.Id.ToString())
                .ToList();

            await _chatHubContext.Clients.Users(userIds).SendAsync(
                "ReceiveNewMessage",
                new { ChatId = chatId }
            );
        }

        return message;
    }

    /// <summary>
    /// Given a chat id, if the current user is a participant will
    /// update the last read date for the current user
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <returns>The updated chat</returns>
    public async Task<Result<Chat>> ReadChat(Guid chatId)
    {
        Result<Guid> userId = _userService.GetCurrentUserId();

        if (!userId.Success)
        {
            return userId.Error;
        }

        return await ValidateId(chatId)
            .Then(() => GetUserChatById(userId.Value, chatId))
            .Then(chat => ReadChat(chat, userId.Value))
            .Then(SaveChanges);
    }

    /// <summary>
    /// Gets the current user's unread message count in all their chats
    /// </summary>
    /// <returns>The unread message count</returns>
    public async Task<Result<int>> GetUnreadChatCount()
    {
        return await _userService.GetCurrentUserId()
            .Then(GetUnreadChatCount);
    }

    /// <summary>
    /// Given a user id, will return the chats the user is part of
    /// in descending order from the last modified date.
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The user's chats</returns>
    private async Task<Result<PagedList<Chat>>> GetChats(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Chats
            .Include(chat => chat.Users)
            .Include(chat => chat.Messages)
            .Where(chat => chat.Users.Any(user => user.Id == userId))
            .OrderByDescending(chat => chat.Messages
                    .OrderByDescending(message => message.CreatedDate)
                    .First().CreatedDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a user's id, will compute the number of unread messages
    /// they have in all their chats.
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <returns>The user's unread message count</returns>
    private async Task<Result<int>> GetUnreadChatCount(Guid userId)
    {
        List<Chat> chats = await _bPContext.Chats
            .Include(chat => chat.Users)
            .Include(chat => chat.Messages)
            .Where(chat =>
                // Check user is in chat
                chat.Users.Any(user => user.Id == userId)
                // Check last message is not from user
                && chat.Messages
                        .OrderByDescending(message => message.CreatedDate)
                        .First().UserId != userId
                // Check last message sent after user last read chat
                && (chat.UserLastReadDate
                    .SingleOrDefault(chatUserRead => chatUserRead.UserId == userId) == null
                        ? DateTimeOffset.MinValue
                        : chat.UserLastReadDate
                            .Single(chatUserRead => chatUserRead.UserId == userId).LastReadDate)
                    < chat.Messages
                        .OrderByDescending(message => message.CreatedDate)
                        .First().CreatedDate)
            .ToListAsync();

        int unreadCount = 0;

        foreach (Chat chat in chats)
        {
            unreadCount += Chat.GetChatUnreadCount(chat, userId);
        }

        return unreadCount;
    }

    /// <summary>
    /// Given a user id and chat id, will validate if the chat exists
    /// and if the user is a participant
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="chatId">The chat's id</param>
    /// <returns>The user's id</returns>
    private async Task<Result<Guid>> ValidateUserInChat(Guid userId, Guid chatId)
    {
        Chat? chat = await _bPContext.Chats
            .Include(chat => chat.Users)
            .FirstOrDefaultAsync(chat => chat.Id == chatId);

        if (chat is null)
        {
            return Chat.Errors.ChatNotFound;
        }

        if (!chat.Users.Any(user => user.Id == userId))
        {
            return Chat.Errors.ChatNotFound;
        }

        return userId;
    }

    /// <summary>
    /// Given a user id and chat id, will validate if the chat exists
    /// and if the user is a participant. Additionally, if it is a private
    /// chat, will validate the participants are still friends.
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="chatId">The chat's id</param>
    /// <returns>The user's id</returns>
    private async Task<Result<Guid>> ValidateChatMessageCreatable(Guid userId, Guid chatId)
    {
        Chat? chat = await _bPContext.Chats
            .Include(chat => chat.Users)
            .FirstOrDefaultAsync(chat => chat.Id == chatId);

        if (chat is null)
        {
            return Chat.Errors.ChatNotFound;
        }

        if (!chat.Users.Any(user => user.Id == userId))
        {
            return Chat.Errors.ChatNotFound;
        }

        // If Private Chat
        if (chat.Users.Count() == 2)
        {
            Guid friendId = chat.Users.First(user => user.Id != userId).Id;

            return await ValidateFriends(userId, friendId);
        }

        return userId;
    }

    /// <summary>
    /// Given a user id and chat id, will get the chat with that id
    /// given the user is a participant
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="chatId">The chat's id</param>
    /// <returns>The chat</returns>
    private async Task<Result<Chat>> GetUserChatById(Guid userId, Guid chatId)
    {
        Chat? chat = await _bPContext.Chats
            .Include(chat => chat.Users)
                .ThenInclude(user => user.SentUserRelations)
            .Include(chat => chat.Users)
                .ThenInclude(user => user.ReceivedUserRelations)
            .Include(chat => chat.Messages)
            .Include(chat => chat.UserLastReadDate)
            .FirstOrDefaultAsync(chat => chat.Id == chatId);

        if (chat is null)
        {
            return Chat.Errors.ChatNotFound;
        }

        if (!chat.Users.Any(user => user.Id == userId))
        {
            return Chat.Errors.ChatNotFound;
        }

        return chat;
    }

    /// <summary>
    /// Given a chat id, will get the chat messages
    /// </summary>
    /// <param name="chatId">The chat's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The chat messages</returns>
    private async Task<Result<PagedList<ChatMessage>>> GetValidatedChatMessages(Guid chatId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.ChatMessages
            .Where(message => message.ChatId == chatId)
            .OrderByDescending(message => message.CreatedDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a user's id and a friend's id, will validate whether 
    /// they are actually friends
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="friendId">The friend's id</param>
    /// <returns>The user's id</returns>
    private async Task<Result<Guid>> ValidateFriends(Guid userId, Guid friendId)
    {
        UserRelation? friendRelation = await _bPContext.UserRelations
            .Include(relation => relation.SentBy)
            .Include(relation => relation.SentTo)
            .Where(relation =>
                relation.RelationType == UserRelationType.Friend
                && ((relation.SentById == userId
                && relation.SentToId == friendId)
                || (relation.SentById == friendId
                && relation.SentToId == userId)))
            .FirstOrDefaultAsync();

        if (friendRelation is null)
        {
            return Chat.Errors.UsersNotFriends;
        }

        return userId;
    }

    /// <summary>
    /// Given a user id and a friend id, will validate whether they are friends
    /// and whether a private chat between the two already exists.
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="friendId">The friend's id</param>
    /// <returns>The chat users</returns>
    private async Task<Result<ChatUsers>> ValidatePrivateChatCreatable(Guid userId, Guid friendId)
    {
        UserRelation? friendRelation = await _bPContext.UserRelations
            .Include(relation => relation.SentBy)
            .Include(relation => relation.SentTo)
            .Where(relation =>
                relation.RelationType == UserRelationType.Friend
                && ((relation.SentById == userId
                && relation.SentToId == friendId)
                || (relation.SentById == friendId
                && relation.SentToId == userId)))
            .FirstOrDefaultAsync();

        if (friendRelation is null)
        {
            return Chat.Errors.UsersNotFriends;
        }

        Chat? existingChat = await _bPContext.Chats
            .Include(chat => chat.Users)
            .Where(chat =>
                chat.Users.Count() == 2
                && chat.Users.All(user => user.Id == userId || user.Id == friendId))
            .FirstOrDefaultAsync();

        if (existingChat != null)
        {
            return Chat.Errors.ChatExists;
        }

        if (friendRelation.SentById == userId)
        {
            return new ChatUsers(friendRelation.SentBy, friendRelation.SentTo);
        }

        return new ChatUsers(friendRelation.SentTo, friendRelation.SentBy);
    }

    /// <summary>
    /// Given a chat and a user's id, will add/ modify the 
    /// chat user read last read date
    /// </summary>
    /// <param name="chat">The chat</param>
    /// <param name="userId">The user's id</param>
    /// <returns>The chat</returns>
    private Result<Chat> ReadChat(Chat chat, Guid userId)
    {
        ChatUserRead? chatUserRead = chat.UserLastReadDate
            .SingleOrDefault(chatUserRead => chatUserRead.UserId == userId);

        if (chatUserRead is null)
        {
            ChatUserRead newChatUserRead = new ChatUserRead()
            {
                ChatId = chat.Id,
                UserId = userId,
                LastReadDate = DateTimeOffset.UtcNow
            };
            chat.UserLastReadDate.Add(newChatUserRead);
            return chat;
        }

        chatUserRead.LastReadDate = DateTimeOffset.UtcNow;
        return chat;
    }

    /// <summary>
    /// Guards if the user id is the same as the relation id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="relationId">The relation id</param>
    /// <returns>The user id</returns>
    private Result<Guid> GuardUserIdsNotEqual(Guid userId, Guid relationId)
    {
        if (userId == relationId)
        {
            return Chat.Errors.RelationIdNotUserId;
        }

        return userId;
    }

    /// <summary>
    /// Checks if a Guid is valid
    /// </summary>
    /// <param name="id">The Id to check</param>
    /// <returns>The Id</returns>
    private Result<Guid> ValidateId(Guid id)
    {
        if (id == default)
        {
            return Chat.Errors.InvalidId;
        }

        return id;
    }

    /// <summary>
    /// Adds the chat message to the database context
    /// </summary>
    /// <param name="message">The chat message</param>
    /// <returns>The chat message</returns>
    private Result<ChatMessage> AddChatMessage(ChatMessage message)
    {
        _bPContext.ChatMessages.Add(message);
        return message;
    }

    /// <summary>
    /// Adds the chat to the database context
    /// </summary>
    /// <param name="chat">The chat</param>
    /// <returns>The chat</returns>
    private Result<Chat> AddChat(Chat chat)
    {
        _bPContext.Chats.Add(chat);
        return chat;
    }

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <param name="message">The message</param>
    /// <returns>The message</returns>
    private async Task<Result<ChatMessage>> SaveChanges(ChatMessage message)
    {
        await _bPContext.SaveChangesAsync();
        return message;
    }

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <param name="chat">The chat</param>
    /// <returns>The chat</returns>
    private async Task<Result<Chat>> SaveChanges(Chat chat)
    {
        await _bPContext.SaveChangesAsync();
        return chat;
    }
}

/// <summary>
/// Class for specifying the users in a chat
/// to be created
/// </summary>
class ChatUsers
{
    public BPUser User;
    public BPUser Friend;

    public ChatUsers(BPUser user, BPUser friend)
    {
        User = user;
        Friend = friend;
    }
}
