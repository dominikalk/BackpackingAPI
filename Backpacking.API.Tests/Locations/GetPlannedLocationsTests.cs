using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Moq;
using Backpacking.API.Models.API;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class GetPlannedLocationsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _unownedLocation = new Mock<Location>().Object;
    private Location _pastLocation = new Mock<Location>().Object;
    private Location _visitedLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;


    public GetPlannedLocationsTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        Guid userId = Guid.NewGuid();

        _unownedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.PlannedLocation
        };

        _pastLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-1),
            LocationType = LocationType.PlannedLocation
        };

        _visitedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.VisitedLocation
        };

        _plannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.PlannedLocation
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());

        _pagingParameters = new BPPagingParameters()
        {
            PageNumber = 1,
            PageSize = 10,
        };
    }

    [TestMethod("[GetPlannedLocations] Unowned Location")]
    public async Task GetPlannedLocations_UnownedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> locations = new List<Location> { _unownedLocation };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetPlannedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.IsNotSubsetOf(locations, result.Value.ToList());
    }

    [TestMethod("[GetPlannedLocations] Location In Past")]
    public async Task GetPlannedLocations_LocationInPast()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> locations = new List<Location> { _pastLocation };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetPlannedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.IsNotSubsetOf(locations, result.Value.ToList());
    }

    [TestMethod("[GetPlannedLocations] Visited Location")]
    public async Task GetPlannedLocations_VisitedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> locations = new List<Location> { _visitedLocation };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetPlannedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.IsNotSubsetOf(locations, result.Value.ToList());
    }

    [TestMethod("[GetPlannedLocations] Success")]
    public async Task GetPlannedLocations_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> expectedLocations = new List<Location> { _plannedLocation };
        List<Location> locations = new List<Location>
        {
            _visitedLocation,
            _pastLocation,
            _unownedLocation,
            _plannedLocation
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetPlannedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedLocations.Count(), result.Value.Count());
        CollectionAssert.IsSubsetOf(expectedLocations, result.Value.ToList());
    }
}
