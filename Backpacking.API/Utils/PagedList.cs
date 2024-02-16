namespace Backpacking.API.Utils;

public interface IPagedList
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
}

public class PagedList : IPagedList
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }

    public PagedList(int pageNumber, int pageSize, int totalPages, int totalCount)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalCount = totalCount;
    }
}

public class PagedList<T> : List<T>, IPagedList
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    public PagedList ToDetails()
    {
        return new PagedList(PageNumber, PageSize, TotalPages, TotalCount);
    }
}
