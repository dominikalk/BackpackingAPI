using Backpacking.API.DbContexts;
using Backpacking.API.Models;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Backpacking.API.Services;

// Default Identity Endpoints Source Code: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs

public class UserService : IUserService
{
    private readonly UserManager<BPUser> _userManager;
    private readonly SignInManager<BPUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBPContext _bPContext;
    private readonly LinkGenerator _linkGenerator;
    private readonly IEmailSender<BPUser> _emailSender;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;

    // We'll figure out a unique endpoint name based on the final route pattern during endpoint generation.
    string? confirmEmailEndpointName = null;

    public UserService(
        UserManager<BPUser> userManager,
        SignInManager<BPUser> signInManager,
        IHttpContextAccessor httpContextAccessor,
        IBPContext bPContext,
        LinkGenerator linkGenerator,
        IEmailSender<BPUser> emailSender,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _bPContext = bPContext;
        _linkGenerator = linkGenerator;
        _emailSender = emailSender;
        _bearerTokenOptions = bearerTokenOptions;
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

        await SendConfirmationEmailAsync(user, registerDTO.Email);
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
    /// Will refresh a user's session given a refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>HttpResult or AccessTokenResponse</returns>
    public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> RefreshToken(string refreshToken)
    {
        var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshToken);

        // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            DateTimeOffset.UtcNow >= expiresUtc ||
            await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not BPUser user)

        {
            return TypedResults.Challenge();
        }

        var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    /// <summary>
    /// Will confirm the users email given the userId and code
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="code">The confirm email code</param>
    /// <returns>HttpResult</returns>
    public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(Guid userId, string code)
    {
        if (await _userManager.FindByIdAsync(userId.ToString()) is not { } user)
        {
            // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
            return TypedResults.Unauthorized();
        }

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return TypedResults.Unauthorized();
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);


        if (!result.Succeeded)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Text("Thank you for confirming your email.");
    }

    /// <summary>
    /// Given an email, will resent the confirmation email
    /// </summary>
    /// <param name="email">The email</param>
    /// <returns>Ok</returns>
    public async Task<Ok> ResendConfirmationEmail(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
        {
            return TypedResults.Ok();
        }

        await SendConfirmationEmailAsync(user, email);
        return TypedResults.Ok();
    }

    /// <summary>
    /// Given an email, will send a forgotten password email
    /// </summary>
    /// <param name="email">The email</param>
    /// <returns>Ok or ValidationProblem</returns>
    public async Task<Results<Ok, ValidationProblem>> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            await _emailSender.SendPasswordResetCodeAsync(user, email, HtmlEncoder.Default.Encode(code));
        }

        // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
        // returned a 400 for an invalid code given a valid user email.
        return TypedResults.Ok();
    }

    /// <summary>
    /// Given an email, the reset code, and a new password, will update the
    /// user's password
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="resetCode">The reset code sent to the email</param>
    /// <param name="newPassword">The new password</param>
    /// <returns>Ok or ValidationProblem</returns>
    public async Task<Results<Ok, ValidationProblem>> ResetPassword(string email, string resetCode, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken()));
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetCode));
            result = await _userManager.ResetPasswordAsync(user, code, newPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.Ok();
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
    /// Given a user and an email, will sent a confirmation email
    /// </summary>
    /// <param name="user">The user to send the email to</param>
    /// <param name="email">The email</param>
    /// <param name="isChange">If the email is changing</param>
    /// <returns>Task</returns>
    private async Task SendConfirmationEmailAsync(BPUser user, string email, bool isChange = false)
    {
        var code = isChange
                ? await _userManager.GenerateChangeEmailTokenAsync(user, email)
                : await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var userId = await _userManager.GetUserIdAsync(user);

        var routeValues = new RouteValueDictionary()
        {
            ["userId"] = userId,
            ["code"] = code,
        };

        if (isChange)
        {
            // This is validated by the /confirmEmail endpoint on change.
            routeValues.Add("changedEmail", email);
        }

        if (_httpContextAccessor.HttpContext is null || confirmEmailEndpointName is null)
        {
            throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");
        }

        var confirmEmailUrl = _linkGenerator.GetUriByName(_httpContextAccessor.HttpContext, confirmEmailEndpointName, routeValues)
            ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

        await _emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
    }

    /// <summary>
    /// Creates a validation proplem from identity result
    /// </summary>
    /// <param name="result">The identity result</param>
    /// <returns>The validation problem</returns>
    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
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
