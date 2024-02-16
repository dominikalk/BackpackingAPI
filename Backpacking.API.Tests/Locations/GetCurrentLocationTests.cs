using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Moq;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class GetCurrentLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _unownedLocation = new Mock<Location>().Object;
    private Location _pastLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;
    private Location _currentLocation = new Mock<Location>().Object;

    public GetCurrentLocationTests()
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

        _pastLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-1),
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

        _currentLocation = new Location()
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
    }

    [TestMethod("[GetCurrentLocation] No Locations")]
    public async Task GetCurrentLocation_NoLocations()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result<Location?> result = await locationService.GetCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(null, result.Value);
    }

    [TestMethod("[GetCurrentLocation] No Owned Locations")]
    public async Task GetCurrentLocation_NoOwnedLocations()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _unownedLocation });

        // Act
        Result<Location?> result = await locationService.GetCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(null, result.Value);
    }

    [TestMethod("[GetCurrentLocation] Location In Past")]
    public async Task GetCurrentLocation_LocationInPast()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _pastLocation });

        // Act
        Result<Location?> result = await locationService.GetCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(null, result.Value);
    }

    [TestMethod("[GetCurrentLocation] Current Planned Location")]
    public async Task GetCurrentLocation_CurrentPlannedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _plannedLocation });

        // Act
        Result<Location?> result = await locationService.GetCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(null, result.Value);
    }


    [TestMethod("[GetCurrentLocation] Current Location Exists")]
    public async Task GetCurrentLocation_CurrentLocationExists()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>
            {
                _pastLocation,
                _currentLocation,
                _unownedLocation,
                _plannedLocation
            });

        // Act
        Result<Location?> result = await locationService.GetCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(_currentLocation, result.Value);
    }
}
