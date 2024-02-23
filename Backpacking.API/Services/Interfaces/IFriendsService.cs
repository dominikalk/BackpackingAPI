using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IFriendsService
{
    Task<Result<PagedList<BPUser>>> SearchUsers(string query, BPPagingParameters pagingParameters);
    Task<Result> SendFriendRequest(Guid requestUserId);
    Task<Result> AcceptFriendRequest(Guid requestId);
    Task<Result> RejectFriendRequest(Guid requestId);
    Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters);
    Task<Result> BlockUser(Guid blockUserId);
    Task<Result> UnblockUser(Guid unblockUserId);
    Task<Result<PagedList<BPUser>>> GetBlockedUsers(BPPagingParameters pagingParameters);
}
