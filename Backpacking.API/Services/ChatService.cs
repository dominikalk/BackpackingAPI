using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.ChatDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Services;

public class ChatService : IChatService
{
    private readonly IBPContext _bPContext;
    private readonly IUserService _userService;

    public ChatService(
        IBPContext bPContext,
        IUserService userService)
    {
        _bPContext = bPContext;
        _userService = userService;
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
    /// Given a friends id and the content of the first message, will
    /// create a private chat with the message given the chat doesn't already
    /// exist and the user's are friends.
    /// </summary>
    /// <param name="createPrivateChatDTO">Then user's id and the message content</param>
    /// <returns>The private chat</returns>
    public async Task<Result<Chat>> CreatePrivateChat(CreatePrivateChatDTO createPrivateChatDTO)
    {
        return await ValidateId(createPrivateChatDTO.UserId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, createPrivateChatDTO.UserId))
            .Then(userId => ValidatePrivateChatCreatable(userId, createPrivateChatDTO.UserId))
            .Then(chatUsers => Chat.CreatePrivateChat(chatUsers.User, chatUsers.Friend, createPrivateChatDTO.Content))
            .Then(AddChat)
            .Then(SaveChanges);
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
        return await ValidateId(chatId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => ValidateUserInChat(userId, chatId))
            .Then(userId => ChatMessage.Create(chatId, userId, createChatMessageDTO.Content))
            .Then(AddChatMessage)
            .Then(SaveChanges);
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
            .Where(chat => chat.Users.Any(user => user.Id == userId))
            .OrderByDescending(chat => chat.LastModifiedDate)
            .ToPagedListAsync(pagingParameters);
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
                && (relation.SentById == userId
                && relation.SentToId == friendId)
                || (relation.SentById == friendId
                && relation.SentToId == userId))
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
    /// Guards if the user id is the same as the relation id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="relationId">The relation id</param>
    /// <returns>The user id</returns>
    private Result<Guid> GuardUserIdsNotEqual(Guid userId, Guid relationId)
    {
        if (userId == relationId)
        {
            return UserRelation.Errors.RelationIdNotUserId;
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
