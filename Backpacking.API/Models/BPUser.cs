using Backpacking.API.Models.DTO.UserDTOs;
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
    /// The chats that the user is part of
    /// </summary>
    public IEnumerable<Chat> Chats { get; set; } = new List<Chat>();

    /// <summary>
    /// The full name of the user
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Given an update profile dto, will update the user and return
    /// the updated user
    /// </summary>
    /// <param name="updateProfileDTO">The update information</param>
    /// <returns>The updated user</returns>
    public Result<BPUser> UpdateUserProfile(UpdateProfileDTO updateProfileDTO)
    {
        FirstName = updateProfileDTO.FirstName;
        LastName = updateProfileDTO.LastName;
        Bio = updateProfileDTO.Bio;

        return this;
    }

    public class Errors
    {
        public static BPError ClaimsNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Claims Not Found");
        public static BPError UserNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Not Found");
        public static BPError UserIdNotFound = new BPError(HttpStatusCode.Unauthorized, "Current User Id Not Found");
        public static BPError UserIdInvalid = new BPError(HttpStatusCode.Unauthorized, "Current User Id Invalid");
    }
}
