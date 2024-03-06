using Backpacking.API.Models;
using Backpacking.API.Models.DTO.UserDTOs;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IUserService
{
    Task<Result<BPUser>> GetCurrentUser();
    Result<Guid> GetCurrentUserId();
    Task<Result<Guid>> RegisterUser(RegisterDTO registerDTO);
    Task<Result> LoginUser(string userName, string password);
    Task<Result<BPUser>> UpdateCurrentUserProfile(UpdateProfileDTO updateProfileDTO);
}
