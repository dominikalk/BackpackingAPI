using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface INetworkService
{
    Task<Result<PagedList<BPUser>>> SearchUsers(string? query, BPPagingParameters pagingParameters);
    Task<Result> UnfriendUser(Guid unfriendUserId);
    Task<Result<PagedList<BPUser>>> GetFriends(BPPagingParameters pagingParameters);
    Task<Result<UserRelation>> SendFriendRequest(Guid requestUserId);
    Task<Result<UserRelation>> AcceptFriendRequest(Guid acceptUserId);
    Task<Result> RejectFriendRequest(Guid rejectUserId);
    Task<Result<PagedList<UserRelation>>> GetFriendRequests(BPPagingParameters pagingParameters);
    Task<Result<UserRelation>> BlockUser(Guid blockUserId);
    Task<Result> UnblockUser(Guid unblockUserId);
    Task<Result<PagedList<BPUser>>> GetBlockedUsers(BPPagingParameters pagingParameters);
}
