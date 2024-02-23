using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;

[Route("v1/friend")]
[Authorize]
[ApiController]
public class FriendsV1Controller : ControllerBase
{
    [HttpGet("search")]
    [EndpointName(nameof(SearchUsers))]
    public async Task<IActionResult> SearchUsers(string query)
    {
        return Ok();
    }
}
