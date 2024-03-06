using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class GetUserByIdTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public GetUserByIdTests()
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
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>());

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());
    }

    [TestMethod("[GetUserById] Invalid Id")]
    public async Task GetUserById_InvalidId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<BPUser> result = await networkService.GetUserById(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[GetUserById] Own Id")]
    public async Task GetUserById_OwnId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<BPUser> result = await networkService.GetUserById(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[GetUserById] Relation Blocked")]
    public async Task GetUserById_RelationBlocked()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        Guid queryUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = queryUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<BPUser> result = await networkService.GetUserById(queryUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserNotFound);
    }

    [TestMethod("[GetUserById] Not Found")]
    public async Task GetUserById_NotFound()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<BPUser> result = await networkService.GetUserById(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserNotFound);
    }

    [TestMethod("[GetUserById] Success")]
    public async Task GetUserById_Success()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        BPUser queryUser = new BPUser()
        {
            Id = Guid.NewGuid()
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser>() { queryUser });

        // Act
        Result<BPUser> result = await networkService.GetUserById(queryUser.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value, queryUser);
    }
}
