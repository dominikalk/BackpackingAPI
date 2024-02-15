using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class DepartCurrentLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _currentLocation = new Mock<Location>().Object;

    public DepartCurrentLocationTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        Guid userId = Guid.NewGuid();

        _currentLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            LocationType = LocationType.VisitedLocation
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());
    }

    [TestMethod("[DepartCurrentLocation] NoCurrentLocation")]
    public async Task DepartCurrentLocation_NoCurrentLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result<Location> result = await locationService.DepartCurrentLocation();

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }

    [TestMethod("[DepartCurrentLocation] Success")]
    public async Task DepartCurrentLocation_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        DateTimeOffset previousDepartDate = new DateTimeOffset(_currentLocation.DepartDate.Date);

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _currentLocation });

        // Act
        Result<Location> result = await locationService.DepartCurrentLocation();

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(_currentLocation.Id, result.Value.Id);
        Assert.AreNotEqual(previousDepartDate, result.Value.DepartDate);
    }
}
