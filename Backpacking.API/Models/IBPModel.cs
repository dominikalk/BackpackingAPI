namespace Backpacking.API.Models;

public interface IBPModel
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }
}
