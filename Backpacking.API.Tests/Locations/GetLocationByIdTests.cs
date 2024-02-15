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
    private readonly ILocationService _locationService;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Location _location;
    private readonly Location _unownedLocation;

    public GetLocationByIdTests()
    {
        _location = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
        };

        _unownedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        Mock<IUserService> userServiceMock = new Mock<IUserService>();
        Mock<IBPContext> bPContextMock = new Mock<IBPContext>();

        userServiceMock
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(_userId));

        bPContextMock
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _location, _unownedLocation });

        _locationService = new LocationService(bPContextMock.Object, userServiceMock.Object);
    }

    [TestMethod("[GetLocationById] Invalid Id")]
    public async Task GetLocationById_InvalidId()
    {
        // Arrange
        Guid id = Guid.Empty;

        // Act
        Result<Location> result = await _locationService.GetLocationById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.InvalidId, result.Error);
    }

    [TestMethod("[GetLocationById] Location Not Found")]
    public async Task GetLocationById_LocationNotFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Result<Location> result = await _locationService.GetLocationById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(HttpStatusCode.NotFound, result.Error.Code);
    }

    [TestMethod("[GetLocationById] Location Not Owned By User")]
    public async Task GetLocationById_LocationNotOwnedByUser()
    {
        // Act
        Result<Location> result = await _locationService.GetLocationById(_unownedLocation.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(HttpStatusCode.NotFound, result.Error.Code);
    }

    [TestMethod("[GetLocationById] Success")]
    public async Task GetLocationById_Success()
    {
        // Act
        Result<Location> result = await _locationService.GetLocationById(_location.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, _location);
    }
}

