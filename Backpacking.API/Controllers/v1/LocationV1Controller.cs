using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.LocationDTOs;
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

    [HttpGet("current")]
    [EndpointName(nameof(GetCurrentLocation))]
    public async Task<IActionResult> GetCurrentLocation()
    {
        Result<Location?> response = await _locationService.GetCurrentLocation();

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location? location)
        {
            if (location is null)
            {
                return Ok(new BPApiResult<LocationDTO?>(null, 0, 0));
            }

            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpPost("current")]
    [EndpointName(nameof(LogCurrentLocation))]
    public async Task<IActionResult> LogCurrentLocation(LogCurrentLocationDTO location)
    {
        Result<Location> response = await _locationService.LogCurrentLocation(location);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, apiResult);
        };
    }

    [HttpPatch("current/depart")]
    [EndpointName(nameof(DepartCurrentLocation))]
    public async Task<IActionResult> DepartCurrentLocation()
    {
        Result<Location> response = await _locationService.DepartCurrentLocation();

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return Ok(apiResult);
        };
    }

    [HttpPut("visited/{id:guid}")]
    [EndpointName(nameof(UpdateVisitedLocation))]
    public async Task<IActionResult> UpdateVisitedLocation(Guid id, UpdateVisitedLocationDTO location)
    {
        Result<Location> response = await _locationService.UpdateVisitedLocation(id, location);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return Ok(apiResult);
        };
    }

    [HttpGet("visited")]
    [EndpointName(nameof(GetVisitedLocations))]
    public async Task<IActionResult> GetVisitedLocations([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> response = await _locationService.GetVisitedLocations(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<Location> pagedLocations)
        {
            IEnumerable<LocationDTO> locationDtos = pagedLocations.Select(location => new LocationDTO(location));

            BPPagedApiResult<LocationDTO> pagedApiResult =
                new BPPagedApiResult<LocationDTO>(locationDtos, pagedLocations.ToDetails());

            return Ok(pagedApiResult);
        }
    }

    [HttpPost("planned")]
    [EndpointName(nameof(LogPlannedLocation))]
    public async Task<IActionResult> LogPlannedLocation(LogPlannedLocationDTO location)
    {
        Result<Location> response = await _locationService.LogPlannedLocation(location);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, apiResult);
        };
    }

    [HttpPut("planned/{id:guid}")]
    [EndpointName(nameof(UpdatePlannedLocation))]
    public async Task<IActionResult> UpdatePlannedLocation(Guid id, UpdatePlannedLocationDTO location)
    {
        Result<Location> response = await _locationService.UpdatePlannedLocation(id, location);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Location location)
        {
            BPApiResult<LocationDTO> apiResult =
                new BPApiResult<LocationDTO>(new LocationDTO(location), 1, 1);

            return Ok(apiResult);
        };
    }

    [HttpGet("planned")]
    [EndpointName(nameof(GetPlannedLocations))]
    public async Task<IActionResult> GetPlannedLocations([FromQuery] BPPagingParameters pagingParameters)
    {
        Result<PagedList<Location>> response = await _locationService.GetPlannedLocations(pagingParameters);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(PagedList<Location> pagedLocations)
        {
            IEnumerable<LocationDTO> locationDtos = pagedLocations.Select(location => new LocationDTO(location));

            BPPagedApiResult<LocationDTO> pagedApiResult =
                new BPPagedApiResult<LocationDTO>(locationDtos, pagedLocations.ToDetails());

            return Ok(pagedApiResult);
        }
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

    [HttpDelete("{id:guid}")]
    [EndpointName(nameof(DeleteLocation))]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        Result response = await _locationService.DeleteLocation(id);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return NoContent();
        }
    }
}
