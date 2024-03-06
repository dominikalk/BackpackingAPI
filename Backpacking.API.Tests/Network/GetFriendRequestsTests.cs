using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models.API;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class GetFriendRequestsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetFriendRequestsTests()
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

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetFriendRequests] Omit Not User's Requests")]
    public async Task GetFriendRequests_OmitNotUsersRequest()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = Guid.NewGuid(),
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<UserRelation>> result = await networkService.GetFriendRequests(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetFriendRequests] Omit Not Pending")]
    public async Task GetFriendRequests_OmitNotPending()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = _userId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<UserRelation>> result = await networkService.GetFriendRequests(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetFriendRequests] Return Friend Requests")]
    public async Task GetFriendRequests_ReturnFriendRequest()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = _userId,
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<UserRelation>> result = await networkService.GetFriendRequests(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 1);
    }
}
