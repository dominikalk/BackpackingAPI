
using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Moq;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class UpdatePlannedLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _unownedLocation = new Mock<Location>().Object;
    private Location _visitedLocation = new Mock<Location>().Object;
    private Location _plannedLocation = new Mock<Location>().Object;

    public UpdatePlannedLocationTests()
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
            LocationType = LocationType.PlannedLocation,
            UserId = Guid.NewGuid(),
        };

        _visitedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = userId,
        };

        _plannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.PlannedLocation,
            UserId = userId,
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _unownedLocation, _visitedLocation, _plannedLocation });
    }

    [TestMethod("[UpdatePlannedLocation] Location Not Found")]
    public async Task UpdatePlannedLocation_LocationNotFound()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(Guid.NewGuid(), dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }
    [TestMethod("[UpdatePlannedLocation] Unowned Location")]
    public async Task UpdatePlannedLocation_UnownedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_unownedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationNotFound, result.Error);
    }


    [TestMethod("[UpdatePlannedLocation] Visited Location")]
    public async Task UpdatePlannedLocation_VisitedLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow,
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_visitedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationPlanned, result.Error);
    }

    [TestMethod("[UpdatePlannedLocation] Arrive In Past")]
    public async Task UpdatePlannedLocation_ArriveInPast()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_plannedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveDateFuture, result.Error);
    }

    [TestMethod("[UpdatePlannedLocation] Depart Before Arrive")]
    public async Task UpdatePlannedLocation_DepartBeforeArrive()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
            DepartDate = DateTimeOffset.UtcNow
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_plannedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveBeforeDepart, result.Error);
    }

    [TestMethod("[UpdatePlannedLocation] Success No Depart")]
    public async Task UpdatePlannedLocation_SuccessNoDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_plannedLocation.Id, dto);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(dto.ArriveDate, result.Value.ArriveDate);
    }

    [TestMethod("[UpdatePlannedLocation] Success With Depart")]
    public async Task UpdatePlannedLocation_SuccessWithDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdatePlannedLocationDTO dto = new UpdatePlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(2),
        };

        // Act
        Result<Location> result = await locationService.UpdatePlannedLocation(_plannedLocation.Id, dto);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(dto.ArriveDate, result.Value.ArriveDate);
    }
}
