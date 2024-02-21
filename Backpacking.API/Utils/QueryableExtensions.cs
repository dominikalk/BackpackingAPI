using Backpacking.API.Models.API;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Utils;

public static class QueryableExtensions
{
    /// <summary>
    /// Extends IQueryable and provides a paged list based on paging parameters
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>A paged list based on the paging parameters</returns>
    public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, BPPagingParameters pagingParameters)
    {
        int count = source.Count();
        List<T> items = source
            .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
            .Take(pagingParameters.PageSize)
            .ToList();

        return new PagedList<T>(items, count, pagingParameters.PageNumber, pagingParameters.PageSize);
    }

    /// <summary>
    /// Extends IQueryable and provides a paged list based on paging parameters
    /// </summary>
    /// <param name="pagingParameters">The paging parameters</param>
    /// <returns>A paged list based on the paging parameters</returns>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, BPPagingParameters pagingParameters)
    {
        int count = source.Count();
        List<T> items = await source
            .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
            .Take(pagingParameters.PageSize)
            .ToListAsync();

        return new PagedList<T>(items, count, pagingParameters.PageNumber, pagingParameters.PageSize);
    }
}
