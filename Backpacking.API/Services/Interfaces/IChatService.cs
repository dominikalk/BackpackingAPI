using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.ChatDTOs;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IChatService
{
    Task<Result<PagedList<Chat>>> GetChats(BPPagingParameters pagingParameters);
    Task<Result<Chat>> GetChatById(Guid chatId);
    Task<Result<PagedList<ChatMessage>>> GetChatMessages(Guid chatId, BPPagingParameters pagingParameters);
    Task<Result<Chat>> CreatePrivateChat(CreatePrivateChatDTO createPrivateChatDTO);
    Task<Result<ChatMessage>> CreateChatMessage(Guid chatId, CreateChatMessageDTO createChatMessageDTO);
    Task<Result<Chat>> ReadChat(Guid chatId);
    Task<Result<int>> GetUnreadChatCount();
}
