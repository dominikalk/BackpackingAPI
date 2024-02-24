using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IFriendsService
{
    Task<Result<PagedList<BPUser>>> SearchUsers(string query, BPPagingParameters pagingParameters);
    Task<Result> UnfriendUser(Guid unfriendId);
    Task<Result<PagedList<BPUser>>> GetFriends(BPPagingParameters pagingParameters);
    Task<Result<UserRelation>> SendFriendRequest(Guid requestUserId);
    Task<Result<UserRelation>> AcceptFriendRequest(Guid requestId);
    Task<Result<UserRelation>> RejectFriendRequest(Guid requestId);
    Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters);
    Task<Result<UserRelation>> BlockUser(Guid blockUserId);
    Task<Result<UserRelation>> UnblockUser(Guid unblockUserId);
    Task<Result<PagedList<BPUser>>> GetBlockedUsers(BPPagingParameters pagingParameters);
}
