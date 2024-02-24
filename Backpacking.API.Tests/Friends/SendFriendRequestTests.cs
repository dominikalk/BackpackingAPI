using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Friends;

[TestClass]
public class SendFriendRequestTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public SendFriendRequestTests()
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

    [TestMethod("[SendFriendRequest] Invalid Id")]
    public async Task SendFriendRequest_InvalidId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[SendFriendRequest] Own Id")]
    public async Task SendFriendRequest_OwnId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        BPUser user = new BPUser()
        {
            Id = _userId
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { user });

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[SendFriendRequest] User Doesn't Exist")]
    public async Task SendFriendRequest_UserDoesntExist()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserNotFound);
    }

    [TestMethod("[SendFriendRequest] Relation Already Exists")]
    public async Task SendFriendRequest_RelationAlreadyExists()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
        };

        BPUser currentUser = new BPUser()
        {
            Id = _userId
        };

        UserRelation relation = new UserRelation()
        {
            SentById = currentUser.Id,
            SentToId = user.Id,
            RelationType = UserRelationType.Pending
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { user, currentUser });

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { relation });

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(user.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserRelationExists);
    }

    [TestMethod("[SendFriendRequest] Relation Blocked By")]
    public async Task SendFriendRequest_RelationBlockedBy()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
        };

        BPUser currentUser = new BPUser()
        {
            Id = _userId
        };

        UserRelation relation = new UserRelation()
        {
            SentById = user.Id,
            SentToId = currentUser.Id,
            RelationType = UserRelationType.Blocked
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { user, currentUser });

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { relation });

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(user.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserBlockedOrBlocking);
    }

    [TestMethod("[SendFriendRequest] Relation Blocking")]
    public async Task SendFriendRequest_RelationBlocking()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
        };

        BPUser currentUser = new BPUser()
        {
            Id = _userId
        };

        UserRelation relation = new UserRelation()
        {
            SentById = currentUser.Id,
            SentToId = user.Id,
            RelationType = UserRelationType.Blocked
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { user, currentUser });

        _mock.Mock<IBPContext>()
           .Setup(context => context.UserRelations)
           .ReturnsDbSet(new List<UserRelation> { relation });

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(user.Id);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.UserBlockedOrBlocking);
    }

    [TestMethod("[SendFriendRequest] Success")]
    public async Task SendFriendRequest_Success()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        BPUser user = new BPUser()
        {
            Id = Guid.NewGuid(),
        };

        BPUser currentUser = new BPUser()
        {
            Id = _userId
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.Users)
            .ReturnsDbSet(new List<BPUser> { user, currentUser });

        // Act
        Result<UserRelation> result = await friendsService.SendFriendRequest(user.Id);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.Value.SentById, currentUser.Id);
        Assert.AreEqual(result.Value.SentToId, user.Id);
        Assert.AreEqual(result.Value.RelationType, UserRelationType.Pending);
    }
}
