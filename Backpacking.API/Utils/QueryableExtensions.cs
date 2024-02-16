using Backpacking.API.Models.API;
using Microsoft.EntityFrameworkCore;

namespace Backpacking.API.Utils;

public static class QueryableExtensions
{
    public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, BPPagingParameters pagingParameters)
    {
        int count = source.Count();
        List<T> items = source
            .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
            .Take(pagingParameters.PageSize)
            .ToList();

        return new PagedList<T>(items, count, pagingParameters.PageNumber, pagingParameters.PageSize);
    }

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
