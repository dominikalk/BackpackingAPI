using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Backpacking.API.Models.DTO.LocationDTOs;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class LogCurrentLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    public LogCurrentLocationTests()
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
    }

    [TestMethod("[LogCurrentLocation] NoPreviousCurrentSuccess")]
    public async Task LogCurrentLocation_NoPreviousCurrentSuccess()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        LogCurrentLocationDTO dto = new LogCurrentLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
        };

        // Act
        Result<Location> result = await locationService.LogCurrentLocation(dto);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod("[LogCurrentLocation] WithPreviousCurrentSuccess")]
    public async Task LogCurrentLocation_WithPreviousCurrentSuccess()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        Location previousLocation = new Location()
        {
            Id = Guid.NewGuid(),
            ArriveDate = DateTimeOffset.UtcNow.AddDays(-2)
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { previousLocation });

        LogCurrentLocationDTO dto = new LogCurrentLocationDTO()
        {
            Name = "Test",
            Longitude = 0,
            Latitude = 0,
        };

        // Act
        Result<Location> result = await locationService.LogCurrentLocation(dto);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
