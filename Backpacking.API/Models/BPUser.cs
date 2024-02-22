using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Backpacking.API.Models;

public class BPUser : IdentityUser<Guid>
{
    /// <summary>
    /// The first name of the user
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The last name of the user
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The bio of the user: A short description of the user
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// The date that the user joined the app
    /// </summary>
    public DateTimeOffset JoinedDate { get; set; }

    /// <summary>
    /// The locations belonging to the user
    /// </summary>
    public IEnumerable<Location> Locations { get; set; } = new List<Location>();

    public class Errors
    {
        public static BPError ClaimsNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        public static BPError UserNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Not Found");
        public static BPError UserIdNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Id Not Found");
        public static BPError UserIdInvalid = new BPError(HttpStatusCode.Unauthorized, "Current User Id Invalid");
    }
}
