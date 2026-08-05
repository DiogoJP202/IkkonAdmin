using System.Collections;

namespace IkkonAdmin.Web.Infrastructure.Pagination;

public interface IPagedResult
{
    int TotalCount { get; }
    int Page { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public sealed class PagedResult<T> : IReadOnlyList<T>, IPagedResult
{
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (decimal)pageSize));
        Page = Math.Clamp(page, 1, TotalPages);
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public int Count => Items.Count;
    public T this[int index] => Items[index];

    public static PagedResult<T> Empty(int pageSize = 20) => new([], 0, 1, pageSize);

    public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        return new PagedResult<TResult>(Items.Select(selector).ToList(), TotalCount, Page, PageSize);
    }

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
