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
public class GetVisitedLocationsTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _unownedLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;
    private Location _visitedLocation = new Mock<Location>().Object;
    private BPPagingParameters _pagingParameters = new Mock<BPPagingParameters>().Object;


    public GetVisitedLocationsTests()
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

        _visitedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
            LocationType = LocationType.VisitedLocation
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

    [TestMethod("[GetVisitedLocations] Unowned Location")]
    public async Task GetVisitedLocations_UnownedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> locations = new List<Location> { _unownedLocation };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetVisitedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.IsNotSubsetOf(locations, result.Value.ToList());
    }

    [TestMethod("[GetVisitedLocations] Planned Location")]
    public async Task GetVisitedLocations_PlannedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> locations = new List<Location> { _plannedLocation };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetVisitedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        CollectionAssert.IsNotSubsetOf(locations, result.Value.ToList());
    }

    [TestMethod("[GetVisitedLocations] Success")]
    public async Task GetVisitedLocations_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        List<Location> expectedLocations = new List<Location> { _visitedLocation };
        List<Location> locations = new List<Location>
        {
            _unownedLocation,
            _plannedLocation,
            _visitedLocation
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(locations);

        // Act
        Result<PagedList<Location>> result = await locationService.GetVisitedLocations(_pagingParameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedLocations.Count(), result.Value.Count());
        CollectionAssert.IsSubsetOf(expectedLocations, result.Value.ToList());
    }
}
