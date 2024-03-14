using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models.API;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Chats;

[TestClass]
public class GetChatsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetChatsTests()
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

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetChats] Filter Not Participant")]
    public async Task GetChats_FilterNotParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<PagedList<Chat>> result = await chatService.GetChats(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetChats] Success Participant")]
    public async Task GetChats_SuccessParticipant()
    {
        // Arrange
        ChatService chatService = _mock.Create<ChatService>();

        Chat chat = new Chat()
        {
            Users = new List<BPUser>() { new BPUser() { Id = _userId } },
            Messages = new List<ChatMessage>() { new ChatMessage() }
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Chats)
            .ReturnsDbSet(new List<Chat> { chat });

        // Act
        Result<PagedList<Chat>> result = await chatService.GetChats(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value.Count);
    }
}
