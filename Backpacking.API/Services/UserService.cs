using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
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
        return await GetClaimsPrinciple()
            .Then(GetUser);
    }

    public Result<Guid> GetCurrentUserId()
    {
        return GetClaimsPrinciple()
            .Then(GetUserId)
            .Then(ParseId);
    }

    private Result<ClaimsPrincipal> GetClaimsPrinciple()
    {
        ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;

        if (claimsPrincipal is null)
        {
            return BPUser.Errors.ClaimsNotFound;
        }

        return claimsPrincipal;
    }

    private async Task<Result<BPUser>> GetUser(ClaimsPrincipal principal)
    {
        BPUser? currentUser = await _userManager.GetUserAsync(principal);

        if (currentUser is null)
        {
            return BPUser.Errors.UserNotFound;
        }

        return currentUser;
    }

    private Result<string> GetUserId(ClaimsPrincipal principal)
    {
        string? userId = _userManager.GetUserId(principal);

        if (userId is null)
        {
            return BPUser.Errors.UserIdNotFound;
        }

        return userId;
    }

    private Result<Guid> ParseId(string id)
    {
        if (!Guid.TryParse(id, out Guid parsedGuid))
        {
            return BPUser.Errors.UserIdInvalid;
        }

        return parsedGuid;
    }
}
