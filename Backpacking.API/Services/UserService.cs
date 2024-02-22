using Backpacking.API.Models;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace Backpacking.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<BPUser> _userManager;
    private readonly SignInManager<BPUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        UserManager<BPUser> userManager,
        SignInManager<BPUser> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user
    /// </summary>
    /// <returns>The current user</returns>
    public async Task<Result<BPUser>> GetCurrentUser()
    {
        return await GetClaimsPrinciple()
            .Then(GetUser);
    }

    /// <summary>
    /// Gets the Id of the current user
    /// </summary>
    /// <returns>The Id of the current user</returns>
    public Result<Guid> GetCurrentUserId()
    {
        return GetClaimsPrinciple()
            .Then(GetUserId)
            .Then(ParseId);
    }

    /// <summary>
    /// Given the registration information, will register a user to the application
    /// </summary>
    /// <param name="registerDTO">The information of the registering user</param>
    /// <returns>The new user's Id</returns>
    public async Task<Result<Guid>> RegisterUser(RegisterDTO registerDTO)
    {
        BPUser user = new BPUser()
        {
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName,
            Email = registerDTO.Email,
            UserName = registerDTO.UserName,
            PasswordHash = registerDTO.Password,
            JoinedDate = DateTimeOffset.UtcNow
        };

        IdentityResult result = await _userManager.CreateAsync(user, user.PasswordHash);

        if (result.Succeeded)
        {
            return user.Id;
        }

        return new BPError(HttpStatusCode.BadRequest, String.Join(" ", result.Errors.Select(error => error.Description)));
    }

    /// <summary>
    /// Will log in the user with the details provided given they are correct
    /// </summary>
    /// <param name="userName">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>Ok Result</returns>
    public async Task<Result> LoginUser(string userName, string password)
    {
        SignInResult result = await _signInManager.PasswordSignInAsync(
            userName,
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Result.Ok();
        }

        if (result.IsLockedOut)
        {
            return new BPError(HttpStatusCode.BadRequest, "User locked out.");
        }

        return new BPError(HttpStatusCode.BadRequest, "Failed to log in.");
    }

    /// <summary>
    /// Will get the claims principle of the current user
    /// </summary>
    /// <returns>The claims principle of the current user</returns>
    private Result<ClaimsPrincipal> GetClaimsPrinciple()
    {
        ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;

        if (claimsPrincipal is null)
        {
            return BPUser.Errors.ClaimsNotFound;
        }

        return claimsPrincipal;
    }

    /// <summary>
    /// Given a claims principal, will get the user
    /// </summary>
    /// <param name="principal">The claims principle</param>
    /// <returns>The user</returns>
    private async Task<Result<BPUser>> GetUser(ClaimsPrincipal principal)
    {
        BPUser? currentUser = await _userManager.GetUserAsync(principal);

        if (currentUser is null)
        {
            return BPUser.Errors.UserNotFound;
        }

        return currentUser;
    }

    /// <summary>
    /// Given a claims principal, will get the id of the user
    /// </summary>
    /// <param name="principal">The claims principle</param>
    /// <returns>The user's Id</returns>
    private Result<string> GetUserId(ClaimsPrincipal principal)
    {
        string? userId = _userManager.GetUserId(principal);

        if (userId is null)
        {
            return BPUser.Errors.UserIdNotFound;
        }

        return userId;
    }

    /// <summary>
    /// Will validate the string id provided and parse into a Guid
    /// if possible. Otherwise an error is returned
    /// </summary>
    /// <param name="id">The id to parse</param>
    /// <returns>The parsed Guid</returns>
    private Result<Guid> ParseId(string id)
    {
        if (!Guid.TryParse(id, out Guid parsedGuid))
        {
            return BPUser.Errors.UserIdInvalid;
        }

        return parsedGuid;
    }
}
