using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using System.Net;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class GetLocationByIdTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _location = new Mock<Location>().Object;
    private Location _unownedLocation = new Mock<Location>().Object;

    public GetLocationByIdTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        Guid userId = Guid.NewGuid();

        _location = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
        };

        _unownedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _location, _unownedLocation });
    }

    [TestMethod("[GetLocationById] Invalid Id")]
    public async Task GetLocationById_InvalidId()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        Guid id = Guid.Empty;

        // Act
        Result<Location> result = await locationService.GetLocationById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.InvalidId, result.Error);
    }

    [TestMethod("[GetLocationById] Location Not Found")]
    public async Task GetLocationById_LocationNotFound()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        Guid id = Guid.NewGuid();

        // Act
        Result<Location> result = await locationService.GetLocationById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }

    [TestMethod("[GetLocationById] Location Not Owned By User")]
    public async Task GetLocationById_LocationNotOwnedByUser()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result<Location> result = await locationService.GetLocationById(_unownedLocation.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }

    [TestMethod("[GetLocationById] Success")]
    public async Task GetLocationById_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result<Location> result = await locationService.GetLocationById(_location.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, _location);
    }
}

