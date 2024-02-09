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
            HttpStatusCode.BadRequest => controller.BadRequest(error.Message),
            HttpStatusCode.NotFound => controller.NotFound(error.Message),
            HttpStatusCode.Forbidden => controller.Forbid(error.Message),
            HttpStatusCode.Unauthorized => controller.Unauthorized(error.Message),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

