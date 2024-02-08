using Backpacking.API.Models;
using Backpacking.API.Utils;

namespace Backpacking.API.Services.Interfaces;

public interface ILocationService
{
    Task<Result<Location>> GetLocationById(Guid id);
}
