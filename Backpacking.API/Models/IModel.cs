namespace Backpacking.API.Models;

public interface IModel
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }
}
