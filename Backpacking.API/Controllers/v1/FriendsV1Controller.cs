using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.FriendDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;

[Route("v1/friend")]
[Authorize]
[ApiController]
public class FriendsV1Controller : ControllerBase
{
    private readonly IFriendsService _friendsService;

    public FriendsV1Controller(IFriendsService friendsService)
    {
        _friendsService = friendsService;
    }

    [HttpGet("search")]
    [EndpointName(nameof(SearchUsers))]
    public async Task<IActionResult> SearchUsers([FromQuery] string? query, [FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _friendsService.SearchUsers(query, pagingParameters);

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
        Result response = await _friendsService.UnfriendUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpGet("friends")]
    [EndpointName(nameof(GetFriends))]
    public async Task<IActionResult> GetFriends([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _friendsService.GetFriends(pagingParameters);

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
        Result response = await _friendsService.SendFriendRequest(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpPost("request/accept/{requestId:guid}")]
    [EndpointName(nameof(AcceptFriendRequest))]
    public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
    {
        Result response = await _friendsService.AcceptFriendRequest(requestId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpPost("request/reject/{requestId:guid}")]
    [EndpointName(nameof(RejectFriendRequest))]
    public async Task<IActionResult> RejectFriendRequest(Guid requestId)
    {
        Result response = await _friendsService.RejectFriendRequest(requestId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpGet("requests")]
    [EndpointName(nameof(GetFriendRequests))]
    public async Task<IActionResult> GetFriendRequests([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<UserRelation>> response = await _friendsService.GetFriendRequests(pagingParameters);

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
        Result response = await _friendsService.BlockUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpPost("unblock/{userId:guid}")]
    [EndpointName(nameof(UnblockUser))]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        Result response = await _friendsService.UnblockUser(userId);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }

    [HttpGet("blocked")]
    [EndpointName(nameof(GetBlockedUsers))]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<BPUser>> response = await _friendsService.GetBlockedUsers(pagingParameters);

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
