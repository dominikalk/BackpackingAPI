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
    public async Task<Result<PagedList<BPUser>>> SearchUsers(string query, BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => SearchUsers(query, pagingParameters, userId));
    }

    public async Task<Result> UnfriendUser(Guid unfriendId)
    {
        return Result.Ok();
    }

    public async Task<Result<PagedList<BPUser>>> GetFriends(BPPagingParameters pagingParameters)
    {
        return Result<PagedList<BPUser>>.Ok(new PagedList<BPUser>(new List<BPUser>(), 0, 0, 0));
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
            .Then(userId => GuardBlockedThenExists(userId, requestUserId))
            .Then(userId => AddFriendRequest(userId, requestUserId))
            .Then(SaveChanges);
    }

    public async Task<Result<UserRelation>> AcceptFriendRequest(Guid requestId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    public async Task<Result<UserRelation>> RejectFriendRequest(Guid requestId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    public async Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters)
    {
        return Result<PagedList<UserRelation>>.Ok(new PagedList<UserRelation>(new List<UserRelation>(), 0, 0, 0));
    }

    public async Task<Result<UserRelation>> BlockUser(Guid blockUserId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    public async Task<Result<UserRelation>> UnblockUser(Guid unblockUserId)
    {
        return Result<UserRelation>.Ok(new UserRelation());
    }

    public async Task<Result<PagedList<BPUser>>> GetBlockedUsers(BPPagingParameters pagingParameters)
    {
        return Result<PagedList<BPUser>>.Ok(new PagedList<BPUser>(new List<BPUser>(), 0, 0, 0));
    }

    /// <summary>
    /// Given a query, will search for users that relate to it and that 
    /// are not blocked by or blocking the user with the user id provided
    /// </summary>
    /// <param name="query">The query to search for users by</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <param name="userId">The user id to check the user relation against</param>
    /// <returns>The users searched for by the query</returns>
    private async Task<Result<PagedList<BPUser>>> SearchUsers(string query, BPPagingParameters pagingParameters, Guid userId)
    {
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
    /// Given 2 user ids, will guard whether each of those users has 
    /// blocked the other, then whether the relationship already exists
    /// </summary>
    /// <param name="sentById">The first user id</param>
    /// <param name="sentToId">The second user id</param>
    /// <returns>The first user id</returns>
    private async Task<Result<Guid>> GuardBlockedThenExists(Guid sentById, Guid sentToId)
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
