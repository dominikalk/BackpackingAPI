using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models;
using Backpacking.API.Models.API;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class SearchUsersTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public SearchUsersTests()
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

    [TestMethod("[SearchUsers] Omit Current User")]
    public async Task SearchUsers_OmitCurrentUser()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        BPUser currentUser = new BPUser()
        {
            Id = _userId,
            UserName = "UserName"
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.Users)
           .ReturnsDbSet(new List<BPUser> { currentUser });

        // Act
        Result<PagedList<BPUser>> result = await networkService.SearchUsers(currentUser.UserName!, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[SearchUsers] Omit Blocked User")]
    public async Task SearchUsers_OmitBlockedUser()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            RelationType = UserRelationType.Blocked
        };

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName",
            ReceivedUserRelations = new List<UserRelation> { userRelation }
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.Users)
           .ReturnsDbSet(new List<BPUser> { user });

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.SearchUsers(user.UserName!, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[SearchUsers] Omit If Blocked By User")]
    public async Task SearchUsers_OmitIfBlockedByUser()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentToId = _userId,
            RelationType = UserRelationType.Blocked
        };

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName",
            SentUserRelations = new List<UserRelation> { userRelation }
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.Users)
           .ReturnsDbSet(new List<BPUser> { user });

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<PagedList<BPUser>> result = await networkService.SearchUsers(user.UserName!, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[SearchUsers] None Found")]
    public async Task SearchUsers_NoneFound()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName",
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.Users)
           .ReturnsDbSet(new List<BPUser> { user });

        // Act
        Result<PagedList<BPUser>> result = await networkService.SearchUsers("Test", _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 0);
    }

    [TestMethod("[SearchUsers] Success")]
    public async Task SearchUsers_Success()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName",
        };

        _mock.Mock<IBPContext>()
           .Setup(context => context.Users)
           .ReturnsDbSet(new List<BPUser> { user });

        // Act
        Result<PagedList<BPUser>> result = await networkService.SearchUsers(user.UserName!, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.Count(), 1);
    }
}
