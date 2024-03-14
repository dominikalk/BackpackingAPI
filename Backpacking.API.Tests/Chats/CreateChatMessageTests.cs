using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models.DTO.ChatDTOs;
using Microsoft.AspNetCore.SignalR;
using Backpacking.API.Hubs;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class CreateChatMessageTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private Guid _friendId;
    private Chat _chat = new Mock<Chat>().Object;
    private Chat _notParticipantChat = new Mock<Chat>().Object;

    private CreateChatMessageDTO _dto = new Mock<CreateChatMessageDTO>().Object;

    public CreateChatMessageTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _friendId = Guid.NewGuid();

        _chat = new Chat()
        {
            Id = Guid.NewGuid(),
            Users = new List<BPUser>
            {
                new BPUser() { Id = _userId },
                new BPUser() { Id = _friendId }
            },
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _notParticipantChat = new Chat()
        {
            Id = Guid.NewGuid(),
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _dto = new CreateChatMessageDTO()
        {
            Content = "Message"
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(_userId));

        Mock<IHubClients> mockClients = new Mock<IHubClients>();
        mockClients
            .Setup(clients => clients.Users(It.IsAny<IReadOnlyList<string>>()))
            .Returns(new Mock<IClientProxy>().Object);

        _mock.Mock<IHubContext<ChatHub>>()
            .Setup(context => context.Clients)
            .Returns(mockClients.Object);

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { _chat, _notParticipantChat });

        _mock.Mock<IBPContext>()
            .Setup(context => context.ChatMessages)
            .ReturnsDbSet(new List<ChatMessage>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());
    }

    [TestMethod("[CreateChatMessage] Invalid Chat Id")]
    public async Task CreateChatMessage_InvalidChatId()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.Empty;

        // Act
        Result<ChatMessage> result = await chatService.CreateChatMessage(id, _dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.InvalidId, result.Error);
    }

    [TestMethod("[CreateChatMessage] Chat Not Found")]
    public async Task CreateChatMessage_ChatNotFound()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<ChatMessage> result = await chatService.CreateChatMessage(id, _dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[CreateChatMessage] User Not Participant")]
    public async Task CreateChatMessage_UserNotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<ChatMessage> result = await chatService.CreateChatMessage(_notParticipantChat.Id, _dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.ChatNotFound, result.Error);
    }

    [TestMethod("[CreateChatMessage] Users Not Friends")]
    public async Task CreateChatMessage_UsersNotFriends()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        // Act
        Result<ChatMessage> result = await chatService.CreateChatMessage(_chat.Id, _dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Chat.Errors.UsersNotFriends, result.Error);
    }

    [TestMethod("[CreateChatMessage] Success")]
    public async Task CreateChatMessage_Success()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = _friendId,
            RelationType = UserRelationType.Friend
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        // Act
        Result<ChatMessage> result = await chatService.CreateChatMessage(_chat.Id, _dto);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(_dto.Content, result.Value.Content);
    }
}
