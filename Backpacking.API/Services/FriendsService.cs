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

    public async Task<Result> SendFriendRequest(Guid requestUserId)
    {
        return Result.Ok();
    }

    public async Task<Result> AcceptFriendRequest(Guid requestId)
    {
        return Result.Ok();
    }

    public async Task<Result> RejectFriendRequest(Guid requestId)
    {
        return Result.Ok();
    }

    public async Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters)
    {
        return Result<PagedList<UserRelation>>.Ok(new PagedList<UserRelation>(new List<UserRelation>(), 0, 0, 0));
    }

    public async Task<Result> BlockUser(Guid blockUserId)
    {
        return Result.Ok();
    }

    public async Task<Result> UnblockUser(Guid unblockUserId)
    {
        return Result.Ok();
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
}
