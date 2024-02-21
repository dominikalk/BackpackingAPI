
using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO.LocationDTOs;
using Backpacking.API.Services;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class UpdateVisitedLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public UpdateVisitedLocationTests()
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
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());
    }

    [TestMethod("[UpdateVisitedLocation] Invalid Id")]
    public async Task UpdateVisitedLocation_InvalidId()
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
        Result<Location> result = await locationService.UpdateVisitedLocation(Guid.Empty, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.InvalidId, result.Error);
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

        Location unownedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = Guid.NewGuid(),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { unownedLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(unownedLocation.Id, dto);

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

        Location plannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.PlannedLocation,
            UserId = _userId,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { plannedLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(plannedLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.LocationVisited, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] Arrive Before Previous Arrive")]
    public async Task UpdateVisitedLocation_ArriveBeforePreviousArrive()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-4),
            DepartDate = DateTimeOffset.UtcNow,
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow,
        };

        Location previousLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-3),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { previousLocation, updatingLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveAfterPreviousArrive, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] Depart After Next Depart")]
    public async Task UpdateVisitedLocation_DepartAfterNextDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-1),
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-4),
        };

        Location nextLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-3),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation, nextLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.DepartBeforeNextDepart, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] Arrive After Depart")]
    public async Task UpdateVisitedLocation_ArriveAfterDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-4),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveBeforeDepart, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] Depart In Future")]
    public async Task UpdateVisitedLocation_DepartInFuture()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2),
            DepartDate = DateTimeOffset.UtcNow.AddDays(1),
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-1),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.DepartDatePast, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] No Depart With Next Location")]
    public async Task UpdateVisitedLocation_NoDepartWithNextLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = null
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-4),
        };

        Location nextLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-3),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation, nextLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.DepartBeforeNextDepart, result.Error);
    }

    [TestMethod("[UpdateVisitedLocation] Success No Previous Or Next Location")]
    public async Task UpdateVisitedLocation_SuccessNoPreviousOrNextLocation()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-4),
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-3),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod("[UpdateVisitedLocation] Success No Depart")]
    public async Task UpdateVisitedLocation_SuccessNoDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-5),
            DepartDate = null
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-3),
            DepartDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { updatingLocation });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod("[UpdateVisitedLocation] Success Skip Planned")]
    public async Task UpdateVisitedLocation_SuccessSkipPlanned()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        DateTimeOffset now = DateTimeOffset.UtcNow;

        UpdateVisitedLocationDTO dto = new UpdateVisitedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = now.AddDays(-6),
            DepartDate = now.AddDays(-1)
        };

        Location updatingLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = now.AddDays(-4),
            DepartDate = now.AddDays(-3),
        };

        Location previousLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = now.AddDays(-6),
            DepartDate = now.AddDays(-5),
        };

        Location nextLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.VisitedLocation,
            UserId = _userId,
            ArriveDate = now.AddDays(-2),
            DepartDate = now.AddDays(-1),
        };

        Location previousPlannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.PlannedLocation,
            UserId = _userId,
            ArriveDate = now.AddDays(-4),
            DepartDate = now.AddDays(-4),
        };

        Location nextPlannedLocation = new Location()
        {
            Id = Guid.NewGuid(),
            LocationType = LocationType.PlannedLocation,
            UserId = _userId,
            ArriveDate = now.AddDays(-3),
            DepartDate = now.AddDays(-3),
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>
            {
                previousLocation,
                previousPlannedLocation,
                updatingLocation,
                nextPlannedLocation,
                nextLocation
            });

        // Act
        Result<Location> result = await locationService.UpdateVisitedLocation(updatingLocation.Id, dto);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(previousLocation.DepartDate, updatingLocation.ArriveDate);
        Assert.AreEqual(updatingLocation.DepartDate, nextLocation.ArriveDate);
    }
}
