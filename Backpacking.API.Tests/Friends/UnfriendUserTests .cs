using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models.API;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Friends;

[TestClass]
public class UnfriendUserTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public UnfriendUserTests()
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

    [TestMethod("[UnfriendUser] Invalid Id")]
    public async Task UnfriendUser_InvalidId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnfriendUser(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[UnfriendUser] Own Id")]
    public async Task UnfriendUser_OwnId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnfriendUser(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[UnfriendUser] Relation Doesn't Exist")]
    public async Task UnfriendUser_RelationDoesntExist()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnfriendUser(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[UnfriendUser] Relation Not Friend")]
    public async Task UnfriendUser_RelationNotFriend()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unfriendUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = unfriendUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnfriendUser(unfriendUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationFriend);
    }

    [TestMethod("[UnfriendUser] Relation Blocked")]
    public async Task UnfriendUser_RelationBlocked()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unfriendUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = unfriendUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnfriendUser(unfriendUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[UnfriendUser] Success Sent To Relation")]
    public async Task UnfriendUser_SuccessSentToRelation()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unfriendUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = unfriendUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnfriendUser(unfriendUserId);

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod("[UnfriendUser] Success Sent By Relation")]
    public async Task UnfriendUser_SuccessSentByRelation()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unfriendUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = unfriendUserId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnfriendUser(unfriendUserId);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
