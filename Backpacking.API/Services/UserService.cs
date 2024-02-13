using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace Backpacking.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<BPUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        UserManager<BPUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<BPUser>> GetCurrentUser()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal is null)
        {
            return new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        }


        BPUser? currentUser = await _userManager.GetUserAsync(principal);

        if (currentUser is null)
        {
            return new BPError(HttpStatusCode.Unauthorized, "Current User Not Found");
        }

        return currentUser;
    }

    public Result<Guid> GetCurrentUserId()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal is null)
        {
            return new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        }

        string? currentUserId = _userManager.GetUserId(principal);

        if (currentUserId is null)
        {
            return new BPError(HttpStatusCode.Unauthorized, "Current User Id Not Found");
        }

        return Guid.Parse(currentUserId);
    }
}
