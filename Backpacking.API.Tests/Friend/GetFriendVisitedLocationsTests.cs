using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models.API;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Backpacking.API.Models;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Friend;

[TestClass]
public class GetFriendVisitedLocationsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetFriendVisitedLocationsTests()
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
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetFriendVisitedLocations] Invalid Id")]
    public async Task GetFriendVisitedLocations_InvalidId()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();
        Guid id = Guid.Empty;

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(id, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserRelation.Errors.InvalidId, result.Error);
    }

    [TestMethod("[GetFriendVisitedLocations] Own Id")]
    public async Task GetFriendVisitedLocations_OwnId()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(_userId, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserRelation.Errors.RelationIdNotUserId, result.Error);
    }

    [TestMethod("[GetFriendVisitedLocations] Not Friends")]
    public async Task GetFriendVisitedLocations_NotFriends()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(id, _pagingParameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserRelation.Errors.RelationFriend, result.Error);
    }

    [TestMethod("[GetFriendVisitedLocations] Filter Not Friend")]
    public async Task GetFriendVisitedLocations_FilterNotFriend()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        Guid friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = friendId,
            RelationType = UserRelationType.Friend
        };

        Location location = new Location()
        {
            UserId = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { location });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(friendId, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendVisitedLocations] Filter Planned")]
    public async Task GetFriendVisitedLocations_FilterPlanned()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        Guid friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = friendId,
            RelationType = UserRelationType.Friend
        };

        Location location = new Location()
        {
            UserId = friendId,
            LocationType = LocationType.PlannedLocation
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        _mock.Mock<IBPContext>()
           .Setup(context => context.Locations)
           .ReturnsDbSet(new List<Location> { location });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(friendId, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendVisitedLocations] Success Friend Visited")]
    public async Task GetFriendVisitedLocations_SuccessFriendVisited()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        Guid friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = friendId,
            RelationType = UserRelationType.Friend
        };

        Location location = new Location()
        {
            UserId = friendId,
            LocationType = LocationType.VisitedLocation
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        _mock.Mock<IBPContext>()
           .Setup(context => context.Locations)
           .ReturnsDbSet(new List<Location> { location });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendVisitedLocations(friendId, _pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value.Count);
    }
}
