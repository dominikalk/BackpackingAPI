using Microsoft.AspNetCore.Identity;

namespace Backpacking.API.Models;

public class User : IdentityUser
{
    public Guid? CurrentLocationId { get; set; }
    public Location? CurrentLocation { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }
}
