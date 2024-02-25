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
    public IEnumerable<UserRelation> SentUserRelations { get; set; } = new List<UserRelation>();

    /// <summary>
    /// The requests sent from another user to the user
    /// </summary>
    public IEnumerable<UserRelation> ReceivedUserRelations { get; set; } = new List<UserRelation>();

    /// <summary>
    /// The full name of the user
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// The pending friend requests for the user
    /// </summary>
    [NotMapped]
    public IEnumerable<UserRelation> PendingUserRelations =>
        ReceivedUserRelations.Where(request => request.RelationType == UserRelationType.Pending);

    /// <summary>
    /// The users who are friends with the user
    /// </summary>
    [NotMapped]
    public IEnumerable<BPUser> Friends
    {
        get
        {
            List<BPUser> friends = SentUserRelations
                .Where(friend => friend.RelationType == UserRelationType.Friend)
                .Select(friend => friend.SentTo)
                .ToList();

            friends.AddRange(ReceivedUserRelations
                .Where(friend => friend.RelationType == UserRelationType.Friend)
                .Select(friend => friend.SentBy));

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
