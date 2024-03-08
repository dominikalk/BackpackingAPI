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
public class FriendV1Controller : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendV1Controller(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpGet("current")]
    [EndpointName(nameof(GetFriendsCurrentLocations))]
    public async Task<IActionResult> GetFriendsCurrentLocations([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> response = await _friendService.GetFriendsCurrentLocations(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<Location> pagedLocations)
        {
            IEnumerable<FriendLocationDTO> FriendLocationDTOs = pagedLocations.Select(location => new FriendLocationDTO(location));

            BPPagedApiResult<FriendLocationDTO> pagedApiResult =
                new BPPagedApiResult<FriendLocationDTO>(FriendLocationDTOs, pagedLocations.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpGet("{id:guid}/visited")]
    [EndpointName(nameof(GetFriendVisitedLocations))]
    public async Task<IActionResult> GetFriendVisitedLocations(Guid id, [FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> response = await _friendService.GetFriendVisitedLocations(id, pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<Location> pagedLocations)
        {
            IEnumerable<FriendLocationDTO> FriendLocationDTOs = pagedLocations.Select(location => new FriendLocationDTO(location));

            BPPagedApiResult<FriendLocationDTO> pagedApiResult =
                new BPPagedApiResult<FriendLocationDTO>(FriendLocationDTOs, pagedLocations.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpGet("{id:guid}/planned")]
    [EndpointName(nameof(GetFriendPlannedLocations))]
    public async Task<IActionResult> GetFriendPlannedLocations(Guid id, [FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> response = await _friendService.GetFriendPlannedLocations(id, pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<Location> pagedLocations)
        {
            IEnumerable<FriendLocationDTO> FriendLocationDTOs = pagedLocations.Select(location => new FriendLocationDTO(location));

            BPPagedApiResult<FriendLocationDTO> pagedApiResult =
                new BPPagedApiResult<FriendLocationDTO>(FriendLocationDTOs, pagedLocations.ToDetails());

            return Ok(pagedApiResult);
        }
    }
}
