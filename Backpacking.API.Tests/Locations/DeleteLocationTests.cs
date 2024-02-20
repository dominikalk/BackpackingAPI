
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
public class DeleteLocationTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Location _location = new Mock<Location>().Object;

    public DeleteLocationTests()
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

    [TestMethod("[DeleteLocation] Location Not Found")]
    public async Task DeleteLocation_LocationNotFound()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();
        Guid id = Guid.NewGuid();

        // Act
        Result result = await locationService.DeleteLocation(id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(HttpStatusCode.NotFound, result.Error.Code);
    }

    [TestMethod("[DeleteLocation] Success")]
    public async Task DeleteLocation_Success()
    {
        // Arrange
        LocationService locationService = _mock.Create<LocationService>();

        // Act
        Result result = await locationService.DeleteLocation(_location.Id);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
