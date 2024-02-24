using Backpacking.API.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Backpacking.API.Models;

public class UserRelation : IBPModel
{
    /// <summary>
    /// The id of the friend relation
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who sent the relationship
    /// </summary>
    public Guid SentById { get; init; }

    /// <summary>
    /// The id of the user who recieved the relationship
    /// </summary>
    public Guid SentToId { get; init; }

    /// <summary>
    /// The user who sent the relationship
    /// </summary>
    public BPUser SentBy { get; init; } = default!;

    /// <summary>
    /// The user who recieved the relationship
    /// </summary>
    public BPUser SentTo { get; init; } = default!;

    /// <summary>
    /// The date that the users became friends
    /// </summary>
    public DateTimeOffset? BecameFriendsDate { get; set; }

    /// <summary>
    /// The status of the request
    /// </summary>
    public UserRelationType RelationType { get; set; } = UserRelationType.Pending;

    /// <summary>
    /// The date the friend relation was created on
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The date the friend relation was last modified on
    /// </summary>
    public DateTimeOffset LastModifiedDate { get; set; }

    /// <summary>
    /// Whether the request has been approved
    /// </summary>
    [NotMapped]
    public bool IsFriend => RelationType == UserRelationType.Friend;

    public UserRelation() { }

    public static UserRelation Create(Guid sentById, Guid sentToId)
    {
        UserRelation userRelation = new UserRelation()
        {
            SentById = sentById,
            SentToId = sentToId,
            RelationType = UserRelationType.Pending
        };

        return userRelation;
    }

    public class Errors
    {
        public static BPError InvalidId = new BPError(HttpStatusCode.BadRequest, "Invalid Id.");
        public static BPError UserNotFound = new BPError(HttpStatusCode.NotFound, "User not found.");
        public static BPError UserBlockedOrBlocking = new BPError(HttpStatusCode.NotFound, "User not found.");
        public static BPError UserRelationExists = new BPError(HttpStatusCode.BadRequest, "User relation already exists.");
        public static BPError RelationIdNotUserId = new BPError(HttpStatusCode.BadRequest, "Relation Id cannot be user's Id.");
    }
}

public enum UserRelationType
{
    Pending,
    Friend,
    Blocked
};
