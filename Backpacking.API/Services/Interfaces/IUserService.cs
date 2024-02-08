using Backpacking.API.Models;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface IUserService
{
    Task<Result<BPUser>> GetCurrentUser();
}
