using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Backpacking.API.Models;

public class BPUser : IdentityUser<Guid>
{
    public IEnumerable<Location> Locations { get; set; } = new List<Location>();

    public class Errors
    {
        public static BPError ClaimsNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        public static BPError UserNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Not Found");
        public static BPError UserIdNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Id Not Found");
        public static BPError UserIdInvalid = new BPError(HttpStatusCode.Unauthorized, "Current User Id Invalid");
    }
}
