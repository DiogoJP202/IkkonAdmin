using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Infrastructure.Pagination;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (decimal)request.PageSize));
        var page = Math.Min(request.Page, totalPages);
        request.Page = page;

        var items = await query
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, page, request.PageSize);
    }
}
