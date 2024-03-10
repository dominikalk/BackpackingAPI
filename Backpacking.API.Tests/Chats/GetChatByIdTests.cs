using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class GetChatByIdTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Chat _chat = new Mock<Chat>().Object;
    private Chat _notParticipantChat = new Mock<Chat>().Object;

    public GetChatByIdTests()
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
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _notParticipantChat = new Chat()
        {
            Id = Guid.NewGuid(),
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { _chat, _notParticipantChat });
    }

    [TestMethod("[GetChatById] Invalid Id")]
    public async Task GetChatById_InvalidId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.Empty;

        // Act
        Result<Chat> result = await chatService.GetChatById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.InvalidId, result.Error);
    }

    [TestMethod("[GetChatById] Not Found")]
    public async Task GetChatById_NotFound()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<Chat> result = await chatService.GetChatById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[GetChatById] Not Participant")]
    public async Task GetChatById_NotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.GetChatById(_notParticipantChat.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[GetChatById] Success")]
    public async Task GetChatById_Success()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.GetChatById(_chat.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(_chat.Id, result.Value.Id);
    }
}
