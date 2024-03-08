using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Services;

public class FriendService : IFriendService
{
    private readonly IBPContext _bPContext;
    private readonly IUserService _userService;

    public FriendService(
        IBPContext bPContext,
        IUserService userService)
    {
        _bPContext = bPContext;
        _userService = userService;
    }

    /// <summary>
    /// Will return the current user's friends' current locations
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The current user's friends' current locations</returns>
    public async Task<Result<PagedList<Location>>> GetFriendsCurrentLocations(BPPagingParameters pagingParameters)
    {
        return await _userService.GetCurrentUserId()
            .Then(userId => GetFriendsCurrentLocations(userId, pagingParameters));
    }

    /// <summary>
    /// Given a friend's id, will return the friend's visited locations
    /// </summary>
    /// <param name="friendId">The friend's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The friend's visited locations</returns>
    public async Task<Result<PagedList<Location>>> GetFriendVisitedLocations(Guid friendId, BPPagingParameters pagingParameters)
    {
        return await ValidateId(friendId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, friendId))
            .Then(userId => GuardUserFriends(userId, friendId))
            .Then(_ => GetUserVisitedLocations(friendId, pagingParameters));
    }

    /// <summary>
    /// Given a friend's id, will return that friend's planned locations that are
    /// in the future
    /// </summary>
    /// <param name="friendId">The friend's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The friend's future planned locations</returns>
    public async Task<Result<PagedList<Location>>> GetFriendPlannedLocations(Guid friendId, BPPagingParameters pagingParameters)
    {
        return await ValidateId(friendId)
            .Then(_userService.GetCurrentUserId)
            .Then(userId => GuardUserIdsNotEqual(userId, friendId))
            .Then(userId => GuardUserFriends(userId, friendId))
            .Then(_ => GetUserPlannedLocations(friendId, pagingParameters));
    }

    /// <summary>
    /// Given the current user id, and the paging parameters, will return the
    /// current locations of the user's friends
    /// </summary>
    /// <param name="userId">The current user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The friends' current locations</returns>
    private async Task<Result<PagedList<Location>>> GetFriendsCurrentLocations(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Locations
            .Include(location => location.User)
                .ThenInclude(user => user!.SentUserRelations)
            .Include(location => location.User)
                .ThenInclude(user => user!.ReceivedUserRelations)
            .Where(location =>
                // Check Location Is Current Location
                location.UserId != userId
                && location.DepartDate >= DateTimeOffset.UtcNow
                && location.LocationType == LocationType.VisitedLocation
                // Check Location Belongs to user who is a sent friend relation to the current user
                && (location.User!.SentUserRelations
                    .Any(relation =>
                        relation.SentToId == userId
                        && relation.RelationType == UserRelationType.Friend)
                // OR Location Belongs to user who is a recieved friend relation to the current user
                || location.User!.ReceivedUserRelations
                    .Any(relation =>
                        relation.SentById == userId
                        && relation.RelationType == UserRelationType.Friend)))
            .OrderByDescending(location => location.ArriveDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a user's id, will return the user's visited locations
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The user's visited locations</returns>
    private async Task<Result<PagedList<Location>>> GetUserVisitedLocations(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Locations
            .Where(location =>
                location.UserId == userId
                && location.LocationType == LocationType.VisitedLocation)
            .OrderByDescending(location => location.ArriveDate)
            .ThenByDescending(location => location.DepartDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given a user id, will return that user's planned locations that are
    /// in the future
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>The user's future planned locations</returns>
    private async Task<Result<PagedList<Location>>> GetUserPlannedLocations(Guid userId, BPPagingParameters pagingParameters)
    {
        return await _bPContext.Locations
            .Where(location =>
                location.UserId == userId
                && location.DepartDate >= DateTimeOffset.UtcNow
                && location.LocationType == LocationType.PlannedLocation)
            .OrderBy(location => location.ArriveDate)
            .ThenBy(location => location.DepartDate)
            .ToPagedListAsync(pagingParameters);
    }

    /// <summary>
    /// Given 2 user ids, will guard whether they are friends or not
    /// </summary>
    /// <param name="userId">The first user id</param>
    /// <param name="friendId">The second user id</param>
    /// <returns>The user's id</returns>
    private async Task<Result<Guid>> GuardUserFriends(Guid userId, Guid friendId)
    {
        bool friends = await _bPContext.UserRelations.AnyAsync(relation =>
            relation.RelationType == UserRelationType.Friend
            && (relation.SentById == userId && relation.SentToId == friendId)
            || (relation.SentById == friendId && relation.SentToId == userId));

        if (!friends)
        {
            return UserRelation.Errors.RelationFriend;
        }

        return userId;
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
}
