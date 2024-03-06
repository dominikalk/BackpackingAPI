using Backpacking.API.Models;
using Backpacking.API.Models.API;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Services.Interfaces;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
        Result response = await _userService.LoginUser(loginDTO.UserName, loginDTO.Password);

        return response.Finally(HandleSuccess, this.HandleError);

        IActionResult HandleSuccess()
        {
            return Ok();
        }
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
}
