using Backpacking.API.Models;
using Backpacking.API.Models.DTO;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers;

[Route("v1/location")]
[Authorize]
[ApiController]
public class LocationV1Controller : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationV1Controller(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("{id:guid}")]
    [EndpointName(nameof(GetLocationById))]
    public async Task<IActionResult> GetLocationById(Guid id)
    {

        Result<Location> response = await _locationService.GetLocationById(id);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return Ok(apiResult);
        }
    }
}
