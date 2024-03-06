using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Network;

[TestClass]
public class AcceptFriendRequestTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public AcceptFriendRequestTests()
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

    [TestMethod("[AcceptFriendRequest] Invalid Id")]
    public async Task AcceptFriendRequest_InvalidId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[AcceptFriendRequest] Own Id")]
    public async Task AcceptFriendRequest_OwnId()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[AcceptFriendRequest] Relation Doesn't Exist")]
    public async Task AcceptFriendRequest_RelationDoesntExist()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[AcceptFriendRequest] Relation Not Pending")]
    public async Task AcceptFriendRequest_RelationNotPending()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        Guid acceptUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = acceptUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(acceptUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationPending);
    }

    [TestMethod("[AcceptFriendRequest] Relation Blocked")]
    public async Task AcceptFriendRequest_RelationBlocked()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        Guid acceptUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = acceptUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(acceptUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[AcceptFriendRequest] Success")]
    public async Task AcceptFriendRequest_Success()
    {
        // Arrange
        NetworkService networkService = _mock.Create<NetworkService>();

        Guid acceptUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = acceptUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result<UserRelation> result = await networkService.AcceptFriendRequest(acceptUserId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.SentById, acceptUserId);
        Assert.AreEqual(result.Value.SentToId, _userId);
        Assert.AreEqual(result.Value.RelationType, UserRelationType.Friend);
    }
}
