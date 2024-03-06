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
public class GetBlockedUsersTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetBlockedUsersTests()
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

    [TestMethod("[GetBlockedUsers] Omit Not User's Blocked")]
    public async Task GetBlockedUsers_OmitNotUsersBlocked()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = Guid.NewGuid(),
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetBlockedUsers(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetBlockedUsers] Omit Not Blocked")]
    public async Task GetBlockedUsers_OmitNotBlocked()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetBlockedUsers(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[GetBlockedUsers] Return Blocked")]
    public async Task GetBlockedUsers_ReturnBlocked()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.GetBlockedUsers(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 1);
    }
}
