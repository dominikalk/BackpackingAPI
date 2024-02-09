using Microsoft.AspNetCore.Identity;

namespace Backpacking.API.Models;

public class BPUser : IdentityUser<Guid>
{
    public IEnumerable<Location> Locations { get; set; } = new List<Location>();
}
