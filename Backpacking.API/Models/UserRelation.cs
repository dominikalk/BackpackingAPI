using Backpacking.API.Utils;
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

    public UserRelation() { }

    /// <summary>
    /// Given a sent by and sent to id, will create and return a pending
    /// user relation
    /// </summary>
    /// <param name="sentById">The id of the user the relation is sent by</param>
    /// <param name="sentToId">The id of the user the relation is sent to</param>
    /// <returns>The pending user relation</returns>
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

    /// <summary>
    /// Given a sent by and sent to id, will create and return a blocked
    /// user relation
    /// </summary>
    /// <param name="sentById">The id of the user blocking</param>
    /// <param name="sentToId">The id of the user being blocked</param>
    /// <returns>The blocked user relation</returns>
    public static UserRelation CreateBlocked(Guid sentById, Guid sentToId)
    {
        UserRelation userRelation = new UserRelation()
        {
            SentById = sentById,
            SentToId = sentToId,
            RelationType = UserRelationType.Blocked
        };

        return userRelation;
    }

    /// <summary>
    /// Guards whether the relation is pending. If the relation is blocked, 
    /// will return an error stating the relation does not exist.
    /// </summary>
    /// <param name="userRelation">The relation</param>
    /// <returns>The relation</returns>
    public static Result<UserRelation> GuardPendingRelation(UserRelation userRelation)
    {
        if (userRelation.RelationType == UserRelationType.Blocked)
        {
            return Errors.RelationNotFound;
        }

        if (userRelation.RelationType != UserRelationType.Pending)
        {
            return Errors.RelationPending;
        }

        return userRelation;
    }

    /// <summary>
    /// Guards whether the relation is a friend. If the relation is blocked, 
    /// will return an error stating the relation does not exist.
    /// </summary>
    /// <param name="userRelation">The relation</param>
    /// <returns>The relation</returns>
    public static Result<UserRelation> GuardFriendRelation(UserRelation userRelation)
    {
        if (userRelation.RelationType == UserRelationType.Blocked)
        {
            return Errors.RelationNotFound;
        }

        if (userRelation.RelationType != UserRelationType.Friend)
        {
            return Errors.RelationFriend;
        }

        return userRelation;
    }

    /// <summary>
    /// Guards whether a relation is a blocking relation
    /// </summary>
    /// <param name="userRelation">The relation</param>
    /// <returns>The relation</returns>
    public static Result<UserRelation> GuardBlockingRelation(UserRelation userRelation)
    {
        if (userRelation.RelationType != UserRelationType.Blocked)
        {
            return Errors.RelationBlocked;
        }

        return userRelation;
    }

    /// <summary>
    /// Given a pending user relation, will accept the friend request
    /// </summary>
    /// <param name="userRelation">The user relation</param>
    /// <returns>The accepted user relation</returns>
    public static Result<UserRelation> AcceptRequest(UserRelation userRelation)
    {
        userRelation.RelationType = UserRelationType.Friend;
        userRelation.BecameFriendsDate = DateTimeOffset.UtcNow;
        return userRelation;
    }

    public class Errors
    {
        public static BPError InvalidId = new BPError(HttpStatusCode.BadRequest, "Invalid Id.");
        public static BPError UserNotFound = new BPError(HttpStatusCode.NotFound, "User not found.");
        public static BPError UserBlockedOrBlocking = new BPError(HttpStatusCode.NotFound, "User not found.");
        public static BPError UserRelationExists = new BPError(HttpStatusCode.BadRequest, "User relation already exists.");
        public static BPError RelationIdNotUserId = new BPError(HttpStatusCode.BadRequest, "Relation Id cannot be user's Id.");
        public static BPError RelationNotFound = new BPError(HttpStatusCode.NotFound, "Relation not found.");
        public static BPError RelationPending = new BPError(HttpStatusCode.BadRequest, "Relation must be pending.");
        public static BPError RelationFriend = new BPError(HttpStatusCode.BadRequest, "Relation must be a friend.");
        public static BPError RelationBlocked = new BPError(HttpStatusCode.BadRequest, "Relation must be blocking.");
    }
}

public enum UserRelationType
{
    Pending,
    Friend,
    Blocked
};
