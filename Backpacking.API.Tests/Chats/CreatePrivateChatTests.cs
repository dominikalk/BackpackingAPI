using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models.DTO.ChatDTOs;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class CreatePrivateChatTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public CreatePrivateChatTests()
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

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());
    }

    [TestMethod("[CreatePrivateChat] Invalid Id")]
    public async Task CreatePrivateChat_InvalidId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.CreatePrivateChat(
            new CreatePrivateChatDTO()
            {
                UserId = Guid.Empty,
                Content = "Message"
            });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.InvalidId, result.Error);
    }

    [TestMethod("[CreatePrivateChat] Own Id")]
    public async Task CreatePrivateChat_OwnId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.CreatePrivateChat(
            new CreatePrivateChatDTO()
            {
                UserId = _userId,
                Content = "Message"
            });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.RelationIdNotUserId, result.Error);
    }

    [TestMethod("[CreatePrivateChat] Not Friends")]
    public async Task CreatePrivateChat_NotFriends()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<Chat> result = await chatService.CreatePrivateChat(
            new CreatePrivateChatDTO()
            {
                UserId = Guid.NewGuid(),
                Content = "Message"
            });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.UsersNotFriends, result.Error);
    }

    [TestMethod("[CreatePrivateChat] Chat Exists")]
    public async Task CreatePrivateChat_ChatExists()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Guid friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = friendId,
            RelationType = UserRelationType.Friend
        };

        Chat chat = new Chat()
        {
            Users = new List<BPUser>
            {
                new BPUser { Id = _userId },
                new BPUser { Id = friendId }
            }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<Chat> result = await chatService.CreatePrivateChat(
            new CreatePrivateChatDTO()
            {
                UserId = friendId,
                Content = "Message"
            });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatExists, result.Error);
    }

    [TestMethod("[CreatePrivateChat] Success")]
    public async Task CreatePrivateChat_Success()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Guid friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = friendId,
            SentBy = new BPUser { Id = _userId },
            SentTo = new BPUser { Id = friendId },
            RelationType = UserRelationType.Friend
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        // Act
        Result<Chat> result = await chatService.CreatePrivateChat(
            new CreatePrivateChatDTO()
            {
                UserId = friendId,
                Content = "Message"
            });

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.AreEquivalent(
            new List<Guid> { _userId, friendId },
            result.Value.Users.Select(user => user.Id).ToList());
    }
}
