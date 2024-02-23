using System.ComponentModel.DataAnnotations.Schema;

namespace Backpacking.API.Models;

public class UserRelation : IBPModel
{
    /// <summary>
    /// The id of the friend relation
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The id of the user who sent the relationship
    /// </summary>
    public Guid SentById { get; set; }

    /// <summary>
    /// The id of the user who recieved the relationship
    /// </summary>
    public Guid SentToId { get; set; }

    /// <summary>
    /// The user who sent the relationship
    /// </summary>
    public BPUser SentBy { get; set; } = new BPUser();

    /// <summary>
    /// The user who recieved the relationship
    /// </summary>
    public BPUser SentTo { get; set; } = new BPUser();

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
}

public enum UserRelationType
{
    Pending,
    Friend,
    Blocked
};
