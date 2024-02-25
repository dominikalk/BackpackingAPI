﻿using Autofac.Extras.Moq;
using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Services;
using Backpacking.API.Utils;
using Moq.EntityFrameworkCore;

namespace Backpacking.API.Tests.Friends;

[TestClass]
public class RejectFriendRequestTests
{
    private readonly AutoMock _mock = AutoMock.GetLoose();

    private Guid _userId;

    public RejectFriendRequestTests()
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

    [TestMethod("[RejectFriendRequest] Invalid Id")]
    public async Task RejectFriendRequest_InvalidId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.RejectFriendRequest(Guid.Empty);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.InvalidId);
    }

    [TestMethod("[RejectFriendRequest] Own Id")]
    public async Task RejectFriendRequest_OwnId()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.RejectFriendRequest(_userId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationIdNotUserId);
    }

    [TestMethod("[RejectFriendRequest] Relation Doesn't Exist")]
    public async Task RejectFriendRequest_RelationDoesntExist()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        // Act
        Result result = await friendsService.RejectFriendRequest(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[RejectFriendRequest] Relation Not Pending")]
    public async Task RejectFriendRequest_RelationNotPending()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid rejectUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = rejectUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Friend,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.RejectFriendRequest(rejectUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationPending);
    }

    [TestMethod("[RejectFriendRequest] Relation Blocked")]
    public async Task RejectFriendRequest_RelationBlocked()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid rejectUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = rejectUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Blocked,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.RejectFriendRequest(rejectUserId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Error, UserRelation.Errors.RelationNotFound);
    }

    [TestMethod("[RejectFriendRequest] Success")]
    public async Task RejectFriendRequest_Success()
    {
        // Arrange
        FriendsService friendsService = _mock.Create<FriendsService>();

        Guid rejectUserId = Guid.NewGuid();

        UserRelation userRelation = new UserRelation()
        {
            SentById = rejectUserId,
            SentToId = _userId,
            RelationType = UserRelationType.Pending,
        };

        _mock.Mock<IBPContext>()
            .Setup(context => context.UserRelations)
            .ReturnsDbSet(new List<UserRelation> { userRelation });

        // Act
        Result result = await friendsService.RejectFriendRequest(rejectUserId);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
