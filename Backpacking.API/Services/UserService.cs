using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace Backpacking.API.Services;

// Default Identity Endpoints Source Code: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs

public class UserService : IUserService
{
    private readonly UserManager<BPUser> _userManager;
    private readonly SignInManager<BPUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBPContext _bPContext;

    public UserService(
        UserManager<BPUser> userManager,
        SignInManager<BPUser> signInManager,
        IHttpContextAccessor httpContextAccessor,
        IBPContext bPContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _bPContext = bPContext;
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
    /// Given a username, will return whether a user with that username
    /// already exists
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <returns>Whether the username already exists</returns>
    public async Task<Result<bool>> GetUserNameAvailable(string userName)
    {
        bool taken = await _bPContext.Users.AnyAsync(user => user.UserName!.ToLower() == userName.ToLower());
        return taken == false;
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
    /// <returns>HttpResult or AccessTokenResponse</returns>
    public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginUser(string userName, string password)
    {
        _signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

        SignInResult result = await _signInManager.PasswordSignInAsync(
            userName,
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        return TypedResults.Empty;
    }

    /// <summary>
    /// Will update the current user's profile according to the dto provided
    /// </summary>
    /// <param name="updateProfileDTO">The update information</param>
    /// <returns>The updated user</returns>
    public async Task<Result<BPUser>> UpdateCurrentUserProfile(UpdateProfileDTO updateProfileDTO)
    {
        return await GetCurrentUser()
            .Then(user => user.UpdateUserProfile(updateProfileDTO))
            .Then(SaveChanges);
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

    /// <summary>
    /// Saves the changes made to the database context
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>The user</returns>
    private async Task<Result<BPUser>> SaveChanges(BPUser user)
    {
        await _bPContext.SaveChangesAsync();
        return user;
    }
}
