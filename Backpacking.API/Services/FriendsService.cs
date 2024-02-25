using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Services;

public class FriendsService : IFriendsService
{
    private readonly IBPContext _bPContext;
    private readonly IUserService _userService;

    public FriendsService(
        IBPContext bPContext,
        IUserService userService)
    {
        _bPContext = bPContext;
        _userService = userService;
    }

    /// <summary>
    /// Given a query, will search for users that relate to it and that 
    /// are not blocked by or blocking the current user
    /// </summary>
    /// <param name="query">The query to search for users by</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The users searched for by the query</returns>
    public async Task<Result<PagedList<BPUser>>> SearchUsers(string? query, BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => SearchUsers(query, pagingParameters, userId));
    }

    /// <summary>
    /// Given the id of a friend to unfriend, will remove the user relation
    /// </summary>
    /// <param name="unfriendUserId">The id of the user to unfriend</param>
    /// <returns>Ok Result</returns>
    public async Task<Result> UnfriendUser(Guid unfriendUserId)
    {
        return await ValidateId(unfriendUserId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, unfriendUserId))
            .Then(userId => GetUserRelation(unfriendUserId, userId))
            .Then(GuardRelationExists)
            .Then(UserRelation.GuardFriendRelation)
            .Then(RemoveUserRelation)
            .Then(SaveChanges);
    }

    /// <summary>
    /// Gets the current user's friends
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The current user's friends</returns>
    public async Task<Result<PagedList<BPUser>>> GetFriends(BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => GetFriends(userId, pagingParameters));
    }

    /// <summary>
    /// Given the id of the user to request, will create a pending user relation given
    /// neither user is blocking the other and a user relation does not already exist
    /// </summary>
    /// <param name="requestUserId">The id of the user to request to be friends with</param>
    /// <returns>The new user relation</returns>
    public async Task<Result<UserRelation>> SendFriendRequest(Guid requestUserId)
    {
        return await ValidateId(requestUserId)
            .Then(() => GuardUserExists(requestUserId))
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, requestUserId))
            .Then(userId => GuardNewRelationValid(userId, requestUserId))
            .Then(userId => AddFriendRequest(userId, requestUserId))
            .Then(SaveChanges);
    }

    /// <summary>
    /// Given the id of a user to accept a frienship of, will accept it if it
    /// exists
    /// </summary>
    /// <param name="acceptUserId">The id of the user to accept as a friend</param>
    /// <returns>The relationship</returns>
    public async Task<Result<UserRelation>> AcceptFriendRequest(Guid acceptUserId)
    {
        return await ValidateId(acceptUserId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, acceptUserId))
            .Then(userId => GetUserRelation(acceptUserId, userId))
            .Then(GuardRelationExists)
            .Then(UserRelation.GuardPendingRelation)
            .Then(UserRelation.AcceptRequest)
            .Then(SaveChanges);
    }

    /// <summary>
    /// Given the id of a user to reject a friendship of, will reject it if it
    /// exists
    /// </summary>
    /// <param name="rejectUserId">The id of the user to reject as a friend</param>
    /// <returns>Ok Result</returns>
    public async Task<Result> RejectFriendRequest(Guid rejectUserId)
    {
        return await ValidateId(rejectUserId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, rejectUserId))
            .Then(userId => GetUserRelation(rejectUserId, userId))
            .Then(GuardRelationExists)
            .Then(UserRelation.GuardPendingRelation)
            .Then(RemoveUserRelation)
            .Then(SaveChanges);
    }

    /// <summary>
    /// Gets the current user's pending friend requests
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The relations</returns>
    public async Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => GetFriendRequests(userId, pagingParameters));
    }

    public async Task<Result<UserRelation>> BlockUser(Guid blockUserId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    public async Task<Result<UserRelation>> UnblockUser(Guid unblockUserId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    /// <summary>
    /// Gets the current user's blocked users
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The blocked users</returns>
    public async Task<Result<PagedList<BPUser>>> GetBlockedUsers(BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => GetBlockedUsers(userId, pagingParameters));
    }

    /// <summary>
    /// Given a query, will search for users that relate to it and that 
    /// are not blocked by or blocking the user with the user id provided
    /// </summary>
    /// <param name="query">The query to search for users by</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <param name="userId">The user id to check the user relation against</param>
    /// <returns>The users searched for by the query</returns>
    private async Task<Result<PagedList<BPUser>>> SearchUsers(string? query, BPPagingParameters pagingParameters, Guid userId)
    {
        if (string.IsNullOrEmpty(query))
        {
            return new PagedList<BPUser>(
                new List<BPUser>(),
                0,
                pagingParameters.PageNumber,
                pagingParameters.PageSize);
        }

        return await _bPContext.Users
            .Include(user => user.SentUserRelations)
            .Include(user => user.ReceivedUserRelations)
            .Where(user =>
                user.Id != userId
                && user.UserName != null
                && user.UserName.Contains(query))
            .FilterBlocked(userId)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given the user id and paging parameters, will return the user's 
    /// pending friend requests
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The relations</returns>
    private async Task<Result<PagedList<UserRelation>>> GetFriendRequests(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.UserRelations
            .Include(relation => relation.SentBy)
            .Where(relation =>
                relation.SentToId == userId
                && relation.RelationType == UserRelationType.Pending)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given the user's id and paging parameters, will return the user's
    /// blocked users
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The blocked users</returns>
    private async Task<Result<PagedList<BPUser>>> GetBlockedUsers(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.UserRelations
            .Include(relation => relation.SentTo)
            .Where(relation =>
                relation.SentById == userId
                && relation.RelationType == UserRelationType.Blocked)
            .Select(relation => relation.SentTo)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a user's id and paging parameters, will return the paged
    /// friends results
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The paged user's friends</returns>
    private async Task<Result<PagedList<BPUser>>> GetFriends(Guid userId, BPPagingParameters pagingParameters)
    {
        PagedList<UserRelation> friendRelations = await _bPContext.UserRelations
            .Include(relation => relation.SentBy)
            .Include(relation => relation.SentTo)
            .Where(relation =>
                relation.RelationType == UserRelationType.Friend
                && (relation.SentById == userId || relation.SentToId == userId))
            .ToPagedListAsync(pagingParameters);

        List<BPUser> friends = friendRelations.Select(relation =>
        {
            if (relation.SentById == userId)
            {
                return relation.SentTo;
            }
            return relation.SentBy;
        }).ToList();

        return new PagedList<BPUser>(
            friends,
            friendRelations.TotalCount,
            friendRelations.PageNumber,
            friendRelations.PageSize);
    }

    /// <summary>
    /// Gets the user relation with the provided parameters
    /// </summary>
    /// <param name="sentById">The sent by id</param>
    /// <param name="sentToId">The sent to id</param>
    /// <returns>The relationship</returns>
    private async Task<Result<UserRelation?>> GetUserRelation(Guid sentById, Guid sentToId)
    {
        return await _bPContext.UserRelations
            .Where(relationship =>
                relationship.SentById == sentById
                && relationship.SentToId == sentToId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Guards that the relationship exists
    /// </summary>
    /// <param name="relation">The relationship</param>
    /// <returns>The relationship</returns>
    private Result<UserRelation> GuardRelationExists(UserRelation? relation)
    {
        if (relation is null)
        {
            return UserRelation.Errors.RelationNotFound;
        }

        return relation;
    }

    /// <summary>
    /// Guards whether a user with the id provided exists
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <returns>Ok Result</returns>
    private async Task<Result> GuardUserExists(Guid userId)
    {
        bool exists = await _bPContext.Users.Where(user => user.Id == userId).AnyAsync();

        if (!exists)
        {
            return UserRelation.Errors.UserNotFound;
        }

        return Result.Ok();
    }

    /// <summary>
    /// Given 2 user ids, will guard whether each of those users has 
    /// blocked the other, then whether the relationship already exists
    /// </summary>
    /// <param name="sentById">The first user id</param>
    /// <param name="sentToId">The second user id</param>
    /// <returns>The first user id</returns>
    private async Task<Result<Guid>> GuardNewRelationValid(Guid sentById, Guid sentToId)
    {
        UserRelation? relation = await _bPContext.UserRelations.Where(relation =>
            (relation.SentById == sentById && relation.SentToId == sentToId)
            || (relation.SentById == sentToId && relation.SentToId == sentById))
            .FirstOrDefaultAsync();

        if (relation is null)
        {
            return sentById;
        }

        if (relation.RelationType == UserRelationType.Blocked)
        {
            return UserRelation.Errors.UserBlockedOrBlocking;
        }

        return UserRelation.Errors.UserRelationExists;
    }

    /// <summary>
    /// Given a sent by and a sent to user id, will create and add
    /// the user relation with type pending.
    /// </summary>
    /// <param name="sentById">The sent by user id</param>
    /// <param name="sentToId">The sent to user id</param>
    /// <returns>The created and added user relation</returns>
    private Result<UserRelation> AddFriendRequest(Guid sentById, Guid sentToId)
    {
        UserRelation relation = UserRelation.Create(sentById, sentToId);
        _bPContext.UserRelations.Add(relation);

        return relation;
    }

    /// <summary>
    /// Removes a relation between users
    /// </summary>
    /// <param name="relation">The relation</param>
    /// <returns>The relation</returns>
    private Result<UserRelation> RemoveUserRelation(UserRelation relation)
    {
        _bPContext.UserRelations.Remove(relation);
        return relation;
    }

    /// <summary>
    /// Guards if the user id is the same as the relation id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="relationId">The relation id</param>
    /// <returns>The user id</returns>
    private Result<Guid> GuardUserIdsNotEqual(Guid userId, Guid relationId)
    {
        if (userId == relationId)
        {
            return UserRelation.Errors.RelationIdNotUserId;
        }

        return userId;
    }

    /// <summary>
    /// Checks if a Guid is valid
    /// </summary>
    /// <param name="id">The Id to check</param>
    /// <returns>The Id</returns>
    private Result<Guid> ValidateId(Guid id)
    {
        if (id == default)
        {
            return UserRelation.Errors.InvalidId;
        }

        return id;
    }

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <param name="relation">The user relation</param>
    /// <returns>The user relation</returns>
    private async Task<Result<UserRelation>> SaveChanges(UserRelation relation)
    {
        await _bPContext.SaveChangesAsync();
        return relation;
    }
}
