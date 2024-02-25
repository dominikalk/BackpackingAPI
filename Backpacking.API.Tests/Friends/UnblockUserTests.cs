using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Friends;

[TestClass]
public class UnblockUserTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public UnblockUserTests()
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

    [TestMethod("[UnblockUser] Invalid Id")]
    public async Task UnblockUser_InvalidId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnblockUser(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[UnblockUser] Own Id")]
    public async Task UnblockUser_OwnId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnblockUser(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[UnblockUser] Relation Doesn't Exist")]
    public async Task UnblockUser_RelationDoesntExist()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.UnblockUser(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[UnblockUser] Relation Not Blocked")]
    public async Task UnblockUser_RelationNotFriend()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unblockUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = unblockUserId,
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnblockUser(unblockUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationBlocked);
    }

    [TestMethod("[UnblockUser] Success")]
    public async Task UnblockUser_Success()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid unblockUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = _userId,
            SentToId = unblockUserId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.UnblockUser(unblockUserId);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
