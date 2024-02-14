using Backpacking.API.Models;
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

    [HttpGet]
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

    [HttpPost]
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

    [HttpGet("planned")]
    [EndpointName(nameof(GetPlannedLocations))]
    public async Task<IActionResult> GetPlannedLocations()
    {
        Result<IEnumerable<Location>> response = await _locationService.GetPlannedLocations();

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(IEnumerable<Location> locations)
        {
            BPApiResult<IEnumerable<LocationDTO>> apiResult =
                new BPApiResult<IEnumerable<LocationDTO>>(
                    locations.Select(location => new LocationDTO(location)).ToList(),
                    locations.Count(),
                    1);

            return Ok(apiResult);
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
