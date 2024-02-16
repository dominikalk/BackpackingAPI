using Backpacking.API.Utils;

namespace Backpacking.API.Models.API;

public class BPPagedApiResult<TData>
{
    public IEnumerable<TData> List { get; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public BPPagedApiResult(PagedList<TData> pagedList)
    {
        List = pagedList;
        PageNumber = pagedList.PageNumber;
        PageSize = pagedList.PageSize;
        TotalPages = pagedList.TotalPages;
        TotalCount = pagedList.TotalCount;
    }

    public BPPagedApiResult(IEnumerable<TData> list, PagedList pagedDetails)
    {
        List = list;
        PageNumber = pagedDetails.PageNumber;
        PageSize = pagedDetails.PageSize;
        TotalPages = pagedDetails.TotalPages;
        TotalCount = pagedDetails.TotalCount;
    }
}
