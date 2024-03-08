using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models;
using Backpacking.API.Models.API;

namespace Backpacking.API.Tests.Friend;

[TestClass]
public class GetFriendsCurrentLocationsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private Guid _friendId;
    private Location _pastLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;
    private Location _currentNotFriendLocation = new Mock<Location>().Object;
    private Location _currentLocation = new Mock<Location>().Object;

    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;

    public GetFriendsCurrentLocationsTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _friendId = Guid.NewGuid();

        UserRelation friendRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = _friendId,
            RelationType = UserRelationType.Friend
        };

        BPUser friend = new BPUser()
        {
            Id = _friendId,
            ReceivedUserRelations = new List<UserRelation> { friendRelation }
        };

        _pastLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = _friendId,
            User = friend,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-1),
            LocationType = LocationType.VisitedLocation
        };

        _plannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = _friendId,
            User = friend,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.PlannedLocation
        };

        _currentNotFriendLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            User = new BPUser(),
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.VisitedLocation
        };

        _currentLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = _friendId,
            User = friend,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.VisitedLocation
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(_userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { friendRelation });

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetFriendsCurrentLocations] No Locations")]
    public async Task GetFriendsCurrentLocations_InvalidId()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendsCurrentLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendsCurrentLocations] Filter Past Location")]
    public async Task GetFriendsCurrentLocations_FilterPastLocation()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _pastLocation });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendsCurrentLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendsCurrentLocations] Filter Planned Location")]
    public async Task GetFriendsCurrentLocations_FilterPlannedLocation()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _plannedLocation });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendsCurrentLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendsCurrentLocations] Filter Not Friend Location")]
    public async Task GetFriendsCurrentLocations_FilterNotFriendLocation()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _currentNotFriendLocation });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendsCurrentLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Value.Count);
    }

    [TestMethod("[GetFriendsCurrentLocations] Success Friend Current Location")]
    public async Task GetFriendsCurrentLocations_SuccessFriendCurrentLocation()
    {
        // Arrange
        FriendService friendService = _mock.Create<FriendService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>
            {
                _pastLocation,
                _plannedLocation,
                _currentNotFriendLocation,
                _currentLocation
            });

        // Act
        Result<PagedList<Location>> result = await friendService.GetFriendsCurrentLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Value.Count);
    }
}
