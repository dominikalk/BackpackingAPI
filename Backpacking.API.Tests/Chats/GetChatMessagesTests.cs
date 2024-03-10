using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models.API;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class GetChatMessagesTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Chat _chat = new Mock<Chat>().Object;
    private Chat _notParticipantChat = new Mock<Chat>().Object;

    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetChatMessagesTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        Guid userId = Guid.NewGuid();

        _chat = new Chat()
        {
            Id = Guid.NewGuid(),
            Users = new List<BPUser> { new BPUser() { Id = userId } },
        };

        _notParticipantChat = new Chat()
        {
            Id = Guid.NewGuid(),
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { _chat, _notParticipantChat });

        _mock.Mock<IBPContext>()
            .Setup(context => context.ChatMessages)
            .ReturnsDbSet(new List<ChatMessage>());

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetChatMessages] Invalid Id")]
    public async Task GetChatMessages_InvalidId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.Empty;

        // Act
        Result<PagedList<ChatMessage>> result = await chatService.GetChatMessages(id, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.InvalidId, result.Error);
    }

    [TestMethod("[GetChatMessages] Not Found")]
    public async Task GetChatMessages_NotFound()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<PagedList<ChatMessage>> result = await chatService.GetChatMessages(id, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[GetChatMessages] Not Participant")]
    public async Task GetChatMessages_NotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<PagedList<ChatMessage>> result = await chatService.GetChatMessages(_notParticipantChat.Id, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[GetChatMessages] Success Filter Chat Messages")]
    public async Task GetChatMessages_SuccessFilterChatMessages()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        ChatMessage message = new ChatMessage()
        {
            ChatId = _chat.Id,
        };

        ChatMessage otherChatMessage = new ChatMessage()
        {
            ChatId = Guid.NewGuid(),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.ChatMessages)
            .ReturnsDbSet(new List<ChatMessage> { message, otherChatMessage });

        // Act
        Result<PagedList<ChatMessage>> result = await chatService.GetChatMessages(_chat.Id, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value.Count);
    }
}
