using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IFriendService
{
    Task<Result<PagedList<Location>>> GetFriendsCurrentLocations(BPPagingParameters pagingParameters);
    // TODO: Add GetFriendsPlannedLocations Method
    Task<Result<PagedList<Location>>> GetFriendVisitedLocations(Guid friendId, BPPagingParameters pagingParameters);
    Task<Result<PagedList<Location>>> GetFriendPlannedLocations(Guid friendId, BPPagingParameters pagingParameters);
}
