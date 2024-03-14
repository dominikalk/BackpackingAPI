using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class GetUnreadChatCountTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public GetUnreadChatCountTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(_userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat>());
    }

    [TestMethod("[GetUnreadChatCount] Filter Not Participant")]
    public async Task GetUnreadChatCount_FilterNotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<int> result = await chatService.GetUnreadChatCount();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value);
    }

    [TestMethod("[GetUnreadChatCount] Filter Own Last Message")]
    public async Task GetUnreadChatCount_FilterOwnLastMessage()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Users = new List<BPUser>()
            {
                new BPUser() {Id = _userId}
            },
            Messages = new List<ChatMessage>()
            {
                new ChatMessage() { UserId = _userId }
            }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<int> result = await chatService.GetUnreadChatCount();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value);
    }

    [TestMethod("[GetUnreadChatCount] Filter Message Before Read")]
    public async Task GetUnreadChatCount_FilterMessageBeforeRead()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Users = new List<BPUser>()
            {
                new BPUser() {Id = _userId}
            },
            Messages = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    CreatedDate = DateTimeOffset.UtcNow.AddDays(-1)
                }
            },
            UserLastReadDate = new List<ChatUserRead>()
            {
                new ChatUserRead()
                {
                    UserId = _userId,
                    LastReadDate = DateTimeOffset.UtcNow
                }
            }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<int> result = await chatService.GetUnreadChatCount();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value);
    }

    [TestMethod("[GetUnreadChatCount] Chat Never Read")]
    public async Task GetUnreadChatCount_ChatNeverRead()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Users = new List<BPUser>()
            {
                new BPUser() {Id = _userId}
            },
            Messages = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    CreatedDate = DateTimeOffset.UtcNow.AddDays(-1)
                }
            }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<int> result = await chatService.GetUnreadChatCount();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value);
    }

    [TestMethod("[GetUnreadChatCount] Message After Read")]
    public async Task GetUnreadChatCount_MessageAfterRead()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Users = new List<BPUser>()
            {
                new BPUser() {Id = _userId}
            },
            Messages = new List<ChatMessage>()
            {
                new ChatMessage()
                {
                    CreatedDate = DateTimeOffset.UtcNow
                }
            },
            UserLastReadDate = new List<ChatUserRead>()
            {
                new ChatUserRead()
                {
                    UserId = _userId,
                    LastReadDate = DateTimeOffset.UtcNow.AddDays(-1)
                }
            }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<int> result = await chatService.GetUnreadChatCount();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value);
    }
}
