
using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq;
using Moq.EntityFrameworkCore;
using System.Net;

namespace Backpacking.API.Tests.Locations;

[TestClass]
public class DeleteLocationByIdTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _location = new Mock<Location>().Object;

    public DeleteLocationByIdTests()
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

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Locations)
            .ReturnsDbSet(new List<Location> { _location });
    }

    [TestMethod("[DeleteLocationByIdTests] Location Not Found")]
    public async Task GetLocationById_LocationNotFound()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        Guid id = Guid.NewGuid();

        // Act
        Result result = await locationService.DeleteLocationById(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(HttpStatusCode.NotFound, result.Error.Code);
    }

    [TestMethod("[DeleteLocationByIdTests] Success")]
    public async Task GetLocationById_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result<Location> result = await locationService.GetLocationById(_location.Id);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
