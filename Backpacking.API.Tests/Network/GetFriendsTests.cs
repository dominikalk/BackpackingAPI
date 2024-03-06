using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models.API;
using Moq;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class GetFriendsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetFriendsTests()
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
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetFriends] Omit Unrelated Relation")]
    public async Task GetFriends_OmitUnrelatedRelation()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = Guid.NewGuid(),
            SentById = Guid.NewGuid(),
            RelationType = UserRelationType.Friend
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetFriends(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetFriends] Omit Not Friend Relation")]
    public async Task GetFriends_OmitNotFriendRelation()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = _userId,
            SentById = Guid.NewGuid(),
            RelationType = UserRelationType.Pending
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetFriends(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetFriends] Success Sent To Friend")]
    public async Task GetFriends_SuccessSentToFriend()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = Guid.NewGuid(),
            SentById = _userId,
            RelationType = UserRelationType.Friend
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetFriends(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 1);
    }

    [TestMethod("[GetFriends] Success Sent By Friend")]
    public async Task GetFriends_SuccessSentByFriend()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = _userId,
            SentById = Guid.NewGuid(),
            RelationType = UserRelationType.Friend
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetFriends(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 1);
    }
}
