using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.ChatDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;


[Route("v1/chat")]
[Authorize]
[ApiController]
public class ChatV1Controller : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;

    public ChatV1Controller(
        IChatService chatService,
        IUserService userService)
    {
        _chatService = chatService;
        _userService = userService;
    }

    [HttpGet]
    [EndpointName(nameof(GetChats))]
    public async Task<IActionResult> GetChats([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<PagedList<Chat>> response = await _chatService.GetChats(pagingParameters);

        return response.Finally(pagedChats => HandleSuccess(pagedChats, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(PagedList<Chat> pagedChats, Guid currentUserId)
        {
            IEnumerable<ChatDTO> chatDTOs = pagedChats.Select(chat => new ChatDTO(chat, currentUserId));

            BPPagedApiResult<ChatDTO> pagedApiResult =
                new BPPagedApiResult<ChatDTO>(chatDTOs, pagedChats.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpGet("{id:guid}")]
    [EndpointName(nameof(GetChatById))]
    public async Task<IActionResult> GetChatById(Guid id)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<Chat> response = await _chatService.GetChatById(id);

        return response.Finally(chat => HandleSuccess(chat, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(Chat chat, Guid currentUserId)
        {
            BPApiResult<ChatDTO> apiResult =
                new BPApiResult<ChatDTO>(new ChatDTO(chat, currentUserId), 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpGet("{chatId:guid}/messages")]
    [EndpointName(nameof(GetChatMessages))]
    public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] BPPagingParameters pagingParameters)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<PagedList<ChatMessage>> response = await _chatService.GetChatMessages(chatId, pagingParameters);

        return response.Finally(pagedChatMessages => HandleSuccess(pagedChatMessages, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(PagedList<ChatMessage> pagedChatMessages, Guid currentUserId)
        {
            IEnumerable<ChatMessageDTO> chatDTOs = pagedChatMessages.Select(chat => new ChatMessageDTO(chat, currentUserId));

            BPPagedApiResult<ChatMessageDTO> pagedApiResult =
                new BPPagedApiResult<ChatMessageDTO>(chatDTOs, pagedChatMessages.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpPost("private")]
    [EndpointName(nameof(CreatePrivateChat))]
    public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatDTO createPrivateChatDTO)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<Chat> response = await _chatService.CreatePrivateChat(createPrivateChatDTO);

        return response.Finally(chat => HandleSuccess(chat, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(Chat chat, Guid currentUserId)
        {
            BPApiResult<ChatDTO> apiResult =
                new BPApiResult<ChatDTO>(new ChatDTO(chat, currentUserId), 1, 1);

            return CreatedAtAction(nameof(GetChatById), new { id = chat.Id }, apiResult);
        }
    }

    [HttpPost("{chatId:guid}/message")]
    [EndpointName(nameof(CreateChatMessage))]
    public async Task<IActionResult> CreateChatMessage(Guid chatId, [FromBody] CreateChatMessageDTO createChatMessageDTO)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<ChatMessage> response = await _chatService.CreateChatMessage(chatId, createChatMessageDTO);

        return response.Finally(chat => HandleSuccess(chat, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(ChatMessage chatMessage, Guid currentUserId)
        {
            BPApiResult<ChatMessageDTO> apiResult =
                new BPApiResult<ChatMessageDTO>(new ChatMessageDTO(chatMessage, currentUserId), 1, 1);

            return CreatedAtAction(nameof(GetChatMessages), new { chatId }, apiResult);
        }
    }
}
