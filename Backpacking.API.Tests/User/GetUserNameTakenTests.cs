using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.User;

[TestClass]
public class GetUserNameTakenTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    public GetUserNameTakenTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>());
    }

    [TestMethod("[GetUserNameTaken] Success Taken")]
    public async Task GetUserNameTaken_SuccessTaken()
    {
        // Arrange
        UserService userService = _mock.Create<UserService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName"
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>() { user });

        // Act
        Result<bool> result = await userService.GetUserNameAvailable(user.UserName);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, false);
    }

    [TestMethod("[GetUserNameTaken] Success Taken Capitalization")]
    public async Task GetUserNameTaken_SuccessTakenCapitalization()
    {
        // Arrange
        UserService userService = _mock.Create<UserService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
            UserName = "UserName"
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>() { user });

        // Act
        Result<bool> result = await userService.GetUserNameAvailable(user.UserName.ToUpper());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, false);
    }

    [TestMethod("[GetUserNameTaken] Success Available")]
    public async Task GetUserNameTaken_SuccessAvailable()
    {
        // Arrange
        UserService userService = _mock.Create<UserService>();

        // Act
        Result<bool> result = await userService.GetUserNameAvailable("Test");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, true);
    }
}
