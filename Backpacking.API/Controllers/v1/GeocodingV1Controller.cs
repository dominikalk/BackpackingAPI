using Backpacking.API.Models.API;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Mvc;
using Backpacking.API.Models.Geocoding;
using Microsoft.AspNetCore.Authorization;

namespace Backpacking.API.Controllers.v1;

[Route("v1/geocoding")]
[Authorize]
[ApiController]
public class GeocodingV1Controller : ControllerBase
{
    private readonly IGeocodingService _geocodingService;

    public GeocodingV1Controller(IGeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    [HttpGet("forward")]
    [EndpointName(nameof(ForwardGeocode))]
    public async Task<IActionResult> ForwardGeocode([FromQuery] string query)
    {
        Result<IEnumerable<GeocodingLocation>> response = await _geocodingService.ForwardGeocode(query);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(IEnumerable<GeocodingLocation> locations)
        {
            BPApiResult<IEnumerable<GeocodingLocation>> apiResult =
                new BPApiResult<IEnumerable<GeocodingLocation>>(locations, locations.Count(), locations.Count());

            return Ok(apiResult);
        }
    }

    [HttpGet("reverse")]
    [EndpointName(nameof(ReverseGeocode))]
    public async Task<IActionResult> ReverseGeocode([FromQuery] float longitude, [FromQuery] float latitude)
    {
        Result<IEnumerable<string>> response = await _geocodingService.ReverseGeocode(longitude, latitude);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(IEnumerable<string> locations)
        {
            BPApiResult<IEnumerable<string>> apiResult =
                new BPApiResult<IEnumerable<string>>(locations, locations.Count(), locations.Count());

            return Ok(apiResult);
        }
    }
}
