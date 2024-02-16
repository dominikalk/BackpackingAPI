using Backpacking.API.Models.API;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backpacking.API.Controllers;

internal static class ControllerBaseExtensions
{
    public static IActionResult HandleError(this ControllerBase controller, BPError error)
    {
        return error.Code switch
        {
            HttpStatusCode.BadRequest => controller.BadRequest(new BPApiError(error)),
            HttpStatusCode.NotFound => controller.NotFound(new BPApiError(error)),
            HttpStatusCode.Unauthorized => controller.Unauthorized(new BPApiError(error)),
            HttpStatusCode.Forbidden => controller.Forbid(),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

