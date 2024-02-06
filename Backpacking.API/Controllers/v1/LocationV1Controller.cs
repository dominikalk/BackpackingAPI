using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;

[Route("v1/location")]
[ApiController]
public class LocationV1Controller : ControllerBase
{
    public LocationV1Controller() { }

    [HttpPost]
    [EndpointName(nameof(CreateLocation))]
    public async Task<IActionResult> CreateLocation()
    {
        return Ok();
    }
}
