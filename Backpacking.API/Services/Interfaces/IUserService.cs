using Backpacking.API.Models;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backpacking.API.Services.Interfaces;

public interface IUserService
{
    Task<Result<BPUser>> GetCurrentUser();
    Result<Guid> GetCurrentUserId();
    Task<Result<bool>> GetUserNameAvailable(string userName);
    Task<Result<Guid>> RegisterUser(RegisterDTO registerDTO);
    Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginUser(string userName, string password);
    Task<Result<BPUser>> UpdateCurrentUserProfile(UpdateProfileDTO updateProfileDTO);
}
