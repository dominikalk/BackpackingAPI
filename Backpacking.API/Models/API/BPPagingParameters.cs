namespace Backpacking.API.Models.API;

public class BPPagingParameters
{
    public int PageNumber { get; set; } = 1;

    const int maxPageSize = 50;
    private int _pageSize = 10;
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
