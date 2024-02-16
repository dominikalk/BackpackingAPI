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
public class LogPlannedLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    public LogPlannedLocationTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        Guid userId = Guid.NewGuid();

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
    }

    [TestMethod("[LogPlannedLocation] Arrive In Past")]
    public async Task LogPlannedLocation_ArriveInPast()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        LogPlannedLocationDTO dto = new LogPlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-1)
        };

        // Act
        Result<Location> result = await locationService.LogPlannedLocation(dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveDateFuture, result.Error);
    }

    [TestMethod("[LogPlannedLocation] Depart Before Arrive")]
    public async Task LogPlannedLocation_DepartBeforeArrive()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        LogPlannedLocationDTO dto = new LogPlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
            DepartDate = DateTimeOffset.UtcNow
        };

        // Act
        Result<Location> result = await locationService.LogPlannedLocation(dto);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(Location.Errors.ArriveBeforeDepart, result.Error);
    }

    [TestMethod("[LogPlannedLocation] Success No Depart")]
    public async Task LogPlannedLocation_SuccessNoDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        LogPlannedLocationDTO dto = new LogPlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
        };

        // Act
        Result<Location> result = await locationService.LogPlannedLocation(dto);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod("[LogPlannedLocation] Success With Depart")]
    public async Task LogPlannedLocation_SuccessWithDepart()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        LogPlannedLocationDTO dto = new LogPlannedLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
            ArriveDate = DateTimeOffset.UtcNow.AddDays(1),
            DepartDate = DateTimeOffset.UtcNow.AddDays(2),
        };

        // Act
        Result<Location> result = await locationService.LogPlannedLocation(dto);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
