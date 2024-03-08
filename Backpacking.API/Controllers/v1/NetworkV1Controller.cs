using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.NetworkDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;

[Route("v1/network")]
[Authorize]
[ApiController]
public class NetworkV1Controller : ControllerBase
{
    private readonly INetworkService _networkService;
    private readonly IUserService _userService;

    public NetworkV1Controller(
        INetworkService networkService,
        IUserService userService)
    {
        _networkService = networkService;
        _userService = userService;
    }

    [HttpGet("{id:guid}")]
    [EndpointName(nameof(GetUserProfileById))]
    public async Task<IActionResult> GetUserProfileById(Guid id)
    {
        Result<Guid> currentUserId = _userService.GetCurrentUserId();

        if (!currentUserId.Success)
        {
            return this.HandleError(currentUserId.Error);
        }

        Result<BPUser> response = await _networkService.GetUserById(id);

        return response.Finally((user) => HandleSuccess(user, currentUserId.Value), this.HandleError);

        IActionResult HandleSuccess(BPUser user, Guid currentUserId)
        {
            BPApiResult<UserProfileDTO> apiResult =
                 new BPApiResult<UserProfileDTO>(new UserProfileDTO(user, currentUserId), 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpGet("search")]
    [EndpointName(nameof(SearchUsers))]
    public async Task<IActionResult> SearchUsers([FromQuery] string? query, [FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _networkService.SearchUsers(query, pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<BPUser> pagedSearchUsers)
        {
            IEnumerable<SearchUserDTO> searchUserDtos = pagedSearchUsers.Select(searchUser => new SearchUserDTO(searchUser));

            BPPagedApiResult<SearchUserDTO> pagedApiResult =
                new BPPagedApiResult<SearchUserDTO>(searchUserDtos, pagedSearchUsers.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpPost("unfriend/{userId:guid}")]
    [EndpointName(nameof(UnfriendUser))]
    public async Task<IActionResult> UnfriendUser(Guid userId)
    {
        Result response = await _networkService.UnfriendUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpGet("friends")]
    [EndpointName(nameof(GetFriends))]
    public async Task<IActionResult> GetFriends([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _networkService.GetFriends(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<BPUser> pagedFriends)
        {
            IEnumerable<FriendDTO> friendsDtos = pagedFriends.Select(friend => new FriendDTO(friend));

            BPPagedApiResult<FriendDTO> pagedApiResult =
                new BPPagedApiResult<FriendDTO>(friendsDtos, pagedFriends.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpPost("request/{userId:guid}")]
    [EndpointName(nameof(SendFriendRequest))]
    public async Task<IActionResult> SendFriendRequest(Guid userId)
    {
        Result response = await _networkService.SendFriendRequest(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpPost("request/accept/{requestId:guid}")]
    [EndpointName(nameof(AcceptFriendRequest))]
    public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
    {
        Result response = await _networkService.AcceptFriendRequest(requestId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpPost("request/reject/{requestId:guid}")]
    [EndpointName(nameof(RejectFriendRequest))]
    public async Task<IActionResult> RejectFriendRequest(Guid requestId)
    {
        Result response = await _networkService.RejectFriendRequest(requestId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpGet("requests")]
    [EndpointName(nameof(GetFriendRequests))]
    public async Task<IActionResult> GetFriendRequests([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<UserRelation>> response = await _networkService.GetFriendRequests(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<UserRelation> pagedUserRelations)
        {
            IEnumerable<FriendRequestDTO> friendRequestDtos = pagedUserRelations.Select(relation => new FriendRequestDTO(relation));

            BPPagedApiResult<FriendRequestDTO> pagedApiResult =
                new BPPagedApiResult<FriendRequestDTO>(friendRequestDtos, pagedUserRelations.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpPost("block/{userId:guid}")]
    [EndpointName(nameof(BlockUser))]
    public async Task<IActionResult> BlockUser(Guid userId)
    {
        Result response = await _networkService.BlockUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpPost("unblock/{userId:guid}")]
    [EndpointName(nameof(UnblockUser))]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        Result response = await _networkService.UnblockUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }

    [HttpGet("blocked")]
    [EndpointName(nameof(GetBlockedUsers))]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _networkService.GetBlockedUsers(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<BPUser> pagedBlockedUsers)
        {
            IEnumerable<BlockedUserDTO> blockedUserDtos = pagedBlockedUsers.Select(user => new BlockedUserDTO(user));

            BPPagedApiResult<BlockedUserDTO> pagedApiResult =
                new BPPagedApiResult<BlockedUserDTO>(blockedUserDtos, pagedBlockedUsers.ToDetails());

            return Ok(pagedApiResult);
        }
    }
}
