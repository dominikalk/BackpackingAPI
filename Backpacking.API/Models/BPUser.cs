using Backpacking.API.Utils;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Backpacking.API.Models;

public class BPUser : IdentityUser<Guid>
{
    /// <summary>
    /// The first name of the user
    /// </summary>
    [PersonalData]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// The last name of the user
    /// </summary>
    [PersonalData]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// The bio of the user: A short description of the user
    /// </summary>
    [PersonalData]
    public string? Bio { get; set; }

    /// <summary>
    /// The date that the user joined the app
    /// </summary>
    [PersonalData]
    public DateTimeOffset JoinedDate { get; set; }

    /// <summary>
    /// The locations belonging to the user
    /// </summary>
    public IEnumerable<Location> Locations { get; set; } = new List<Location>();

    /// <summary>
    /// The requests sent from the user to another
    /// </summary>
    public IEnumerable<Friend> SentFriendRequests { get; set; } = new List<Friend>();

    /// <summary>
    /// The requests sent from another user to the user
    /// </summary>
    public IEnumerable<Friend> ReceivedFriendRequests { get; set; } = new List<Friend>();

    /// <summary>
    /// The pending friend requests for the user
    /// </summary>
    [NotMapped]
    public IEnumerable<Friend> PendingFriendRequests =>
        ReceivedFriendRequests.Where(request => request.RequestStatus == FriendRequestStatus.Pending);

    /// <summary>
    /// The users who are friends with the user
    /// </summary>
    [NotMapped]
    public IEnumerable<BPUser> Friends
    {
        get
        {
            List<BPUser> friends = SentFriendRequests.Where(friend => friend.Approved).Select(friend => friend.RequestedTo).ToList();
            friends.AddRange(ReceivedFriendRequests.Where(friend => friend.Approved).Select(friend => friend.RequestedBy));
            return friends;
        }
    }

    public class Errors
    {
        public static BPError ClaimsNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        public static BPError UserNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Not Found");
        public static BPError UserIdNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Id Not Found");
        public static BPError UserIdInvalid = new BPError(HttpStatusCode.Unauthorized, "Current User Id Invalid");
    }
}
