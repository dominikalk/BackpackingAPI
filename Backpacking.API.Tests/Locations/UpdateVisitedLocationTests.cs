
using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Services;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class UpdateVisitedLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _unownedLocation = new Mock<Location>().Object;
    private Location _visitedLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;

    public UpdateVisitedLocationTests()
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
            LocationType = LocationType.VisitedLocation,
            UserId = Guid.NewGuid(),
        };

        _plannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.PlannedLocation,
            UserId = userId,
        };

        _visitedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = userId,
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _unownedLocation, _plannedLocation, _visitedLocation });
    }

    [TestMethod("[UpdateVisitedLocation] Location Not Found")]
    public async Task UpdateVisitedLocation_LocationNotFound()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
            DepartDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(Guid.NewGuid(), dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }
    [TestMethod("[UpdateVisitedLocation] Unowned Location")]
    public async Task UpdateVisitedLocation_UnownedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
            DepartDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(_unownedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }


    [TestMethod("[UpdateVisitedLocation] Planned Location")]
    public async Task UpdateVisitedLocation_PlannedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
            DepartDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(_plannedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationVisited, result.Error);
    }

}
