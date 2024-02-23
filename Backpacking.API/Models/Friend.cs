using System.ComponentModel.DataAnnotations.Schema;

namespace Backpacking.API.Models;

public class Friend : IBPModel
{
    /// <summary>
    /// The id of the friend relation
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The id of the user who requested the friendship
    /// </summary>
    public Guid RequestedById { get; set; }

    /// <summary>
    /// The id of the user who recieved the friendship request
    /// </summary>
    public Guid RequestedToId { get; set; }

    /// <summary>
    /// The user who requested the friendship
    /// </summary>
    public BPUser RequestedBy { get; set; } = new BPUser();

    /// <summary>
    /// The user who recieved the friendship request
    /// </summary>
    public BPUser RequestedTo { get; set; } = new BPUser();

    /// <summary>
    /// The time that the users became friends
    /// </summary>
    public DateTimeOffset? BecameFriendsTime { get; set; }

    /// <summary>
    /// The status of the request
    /// </summary>
    public FriendRequestStatus RequestStatus { get; set; } = FriendRequestStatus.Pending;

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
    public bool Approved => RequestStatus == FriendRequestStatus.Approved;
}

public enum FriendRequestStatus
{
    Pending,
    Approved,
    Blocked
};
