using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;
using Moq;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class BlockUserTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;
    private BPUser _blockUser = new Mock<BPUser>().Object;

    public BlockUserTests()
    {
        _mock = AutoMock.GetLoose();
    }

    [TestInitialize]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        _blockUser = new BPUser()
        {
            Id = Guid.NewGuid()
        };

        _mock.Mock<IUserService>()
            .Setup(service => service.GetCurrentUserId())
            .Returns(Result<Guid>.Ok(_userId));

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { _blockUser });

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation>());
    }

    [TestMethod("[BlockUser] Invalid Id")]
    public async Task BlockUser_InvalidId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.BlockUser(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[BlockUser] Own Id")]
    public async Task BlockUser_OwnId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.BlockUser(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[BlockUser] User Doesn't Exist")]
    public async Task BlockUser_UserDoesntExist()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.BlockUser(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserNotFound);
    }

    [TestMethod("[BlockUser] Current Relation Blocking")]
    public async Task BlockUser_CurrentRelationBlocking()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = _blockUser.Id,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<UserRelation> result = await networkService.BlockUser(_blockUser.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserBlockedOrBlocking);
    }

    [TestMethod("[BlockUser] Success Current Relation Exists")]
    public async Task BlockUser_SuccessCurrentRelationExists()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = _blockUser.Id,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<UserRelation> result = await networkService.BlockUser(_blockUser.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.RelationType, UserRelationType.Blocked);
        Assert.AreEqual(result.Value.SentById, _userId);
        Assert.AreEqual(result.Value.SentToId, _blockUser.Id);
    }

    [TestMethod("[BlockUser] Success No Current Relation")]
    public async Task BlockUser_SuccessNoCurrentRelation()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.BlockUser(_blockUser.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.RelationType, UserRelationType.Blocked);
        Assert.AreEqual(result.Value.SentById, _userId);
        Assert.AreEqual(result.Value.SentToId, _blockUser.Id);
    }
}
