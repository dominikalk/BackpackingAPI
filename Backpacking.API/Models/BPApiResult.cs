namespace Backpacking.API.Models;

public class BPApiResult<TData>
{
    public TData Data { get; set; }
    public int Count { get; set; } = 0;
    public int Total { get; set; } = 0;

    public BPApiResult(TData data, int count, int total)
    {
        Data = data;
        Count = count;
        Total = total;
    }
}
