using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class ReadChatTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Chat _chat = new Mock<Chat>().Object;
    private Chat _notParticipantChat = new Mock<Chat>().Object;

    public ReadChatTests()
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

    [TestMethod("[ReadChat] Invalid Id")]
    public async Task ReadChat_InvalidId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.Empty;

        // Act
        Result<Chat> result = await chatService.ReadChat(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.InvalidId, result.Error);
    }


    [TestMethod("[ReadChat] Not Found")]
    public async Task ReadChat_NotFound()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<Chat> result = await chatService.ReadChat(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[ReadChat] Not Participant")]
    public async Task ReadChat_NotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.ReadChat(_notParticipantChat.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[ReadChat] Success")]
    public async Task ReadChat_Success()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.ReadChat(_chat.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(_chat.Id, result.Value.Id);
        Assert.AreEqual(1, result.Value.UserLastReadDate.Count);
    }
}
