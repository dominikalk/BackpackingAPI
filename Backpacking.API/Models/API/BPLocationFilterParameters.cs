namespace Backpacking.API.Models.API;

public class BPLocationFilterParameters
{
    public DateTimeOffset? FromArrive { get; set; }
    public DateTimeOffset? ToArrive { get; set; }
    public DateTimeOffset? FromDepart { get; set; }
    public DateTimeOffset? ToDepart { get; set; }
    public DateTimeOffset? ArriveOn { get; set; }
    public DateTimeOffset? DepartOn { get; set; }
    public DateTimeOffset? At { get; set; }
}
