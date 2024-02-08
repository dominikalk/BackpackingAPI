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

    public UserService(UserManager<BPUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<BPUser>> GetCurrentUser()
    {
        ClaimsPrincipal? principal = ClaimsPrincipal.Current;

        if (principal is null)
        {
            return new BPError(HttpStatusCode.NotFound, "Current User Not Found");
        }

        BPUser? currentUser = await _userManager.GetUserAsync(principal);

        if (currentUser is null)
        {
            return new BPError(HttpStatusCode.NotFound, "Current User Not Found");
        }

        return currentUser;
    }
}
