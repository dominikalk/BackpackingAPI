using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.ChatDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;

namespace Backpacking.API.Services;

public class ChatService : IChatService
{

    public async Task<Result<PagedList<Chat>>> GetChats(BPPagingParameters pagingParameters)
    {
        return new PagedList<Chat>(new List<Chat>(), 0, 0, 0);
    }
    public async Task<Result<Chat>> GetChatById(Guid chatId)
    {
        return new Chat();
    }
    public async Task<Result<PagedList<ChatMessage>>> GetChatMessages(Guid chatId, BPPagingParameters pagingParameters)
    {
        return new PagedList<ChatMessage>(new List<ChatMessage>(), 0, 0, 0);
    }
    public async Task<Result<Chat>> CreatePrivateChat(CreatePrivateChatDTO createPrivateChatDTO)
    {
        return new Chat();
    }
    public async Task<Result<ChatMessage>> CreateChatMessage(Guid chatId, CreateChatMessageDTO createChatMessageDTO)
    {
        return new ChatMessage();
    }
}
