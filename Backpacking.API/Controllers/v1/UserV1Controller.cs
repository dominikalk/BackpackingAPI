﻿using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backpacking.API.Controllers.v1;

[Route("v1/user")]
[ApiController]
public class UserV1Controller : ControllerBase
{
    private readonly IUserService _userService;

    public UserV1Controller(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("available")]
    [EndpointName(nameof(GetUserNameAvailable))]
    public async Task<IActionResult> GetUserNameAvailable([FromQuery] string userName)
    {
        Result<bool> response = await _userService.GetUserNameAvailable(userName);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(bool available)
        {
            BPApiResult<Object> apiResult =
                new BPApiResult<Object>(new { UserNameAvailable = available }, 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpPost("register")]
    [EndpointName(nameof(Register))]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
        Result<Guid> response = await _userService.RegisterUser(registerDTO);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(Guid id)
        {
            BPApiResult<Object> apiResult =
                new BPApiResult<Object>(new { Id = id }, 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpPost("login")]
    [EndpointName(nameof(Login))]
    public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginDTO loginDTO)
    {
        Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult> response =
            await _userService.LoginUser(loginDTO.UserName, loginDTO.Password);

        return response;
    }

    [HttpPost("refresh")]
    [EndpointName(nameof(Refresh))]
    public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> Refresh(RefreshDTO refreshDTO)
    {
        Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult> response =
            await _userService.RefreshToken(refreshDTO.RefreshToken);

        return response;
    }

    [HttpGet("confirmEmail")]
    [EndpointName(nameof(ConfirmEmail))]
    public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string code, [FromQuery] string? changedEmail)
    {
        Results<ContentHttpResult, UnauthorizedHttpResult> response =
            await _userService.ConfirmEmail(userId, code, changedEmail);

        return response;
    }

    [HttpPost("resendConfirmationEmail")]
    [EndpointName(nameof(ResendConfirmationEmail))]
    public async Task<Ok> ResendConfirmationEmail(ResendConfirmationEmailDTO resendConfirmationEmailDTO)
    {
        Ok response = await _userService.ResendConfirmationEmail(resendConfirmationEmailDTO.Email);

        return response;
    }

    [HttpPost("forgotPassword")]
    [EndpointName(nameof(ForgotPassword))]
    public async Task<Results<Ok, ValidationProblem>> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
    {
        Results<Ok, ValidationProblem> response = await _userService.ForgotPassword(forgotPasswordDTO.Email);

        return response;
    }

    [HttpPost("resetPassword")]
    [EndpointName(nameof(ResetPassword))]
    public async Task<Results<Ok, ValidationProblem>> ResetPassword(ResetPasswordDTO resetPasswordDTO)
    {
        Results<Ok, ValidationProblem> response =
            await _userService.ResetPassword(resetPasswordDTO.Email, resetPasswordDTO.ResetCode, resetPasswordDTO.NewPassword);

        return response;
    }

    [HttpGet("profile")]
    [Authorize]
    [EndpointName(nameof(GetProfile))]
    public async Task<IActionResult> GetProfile()
    {
        Result<BPUser> response = await _userService.GetCurrentUser();

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(BPUser user)
        {
            BPApiResult<ProfileDTO> apiResult =
                new BPApiResult<ProfileDTO>(new ProfileDTO(user), 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpPut("profile")]
    [Authorize]
    [EndpointName(nameof(UpdateProfile))]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDTO updateProfileDTO)
    {
        Result<BPUser> response = await _userService.UpdateCurrentUserProfile(updateProfileDTO);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess(BPUser user)
        {
            BPApiResult<ProfileDTO> apiResult =
                new BPApiResult<ProfileDTO>(new ProfileDTO(user), 1, 1);

            return Ok(apiResult);
        }
    }

    [HttpPost("email")]
    [Authorize]
    [EndpointName(nameof(UpdateEmail))]
    public async Task<IActionResult> UpdateEmail(UpdateEmailDTO updateEmailDTO)
    {
        Result response = await _userService.UpdateEmail(updateEmailDTO.Email);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
    }
}
