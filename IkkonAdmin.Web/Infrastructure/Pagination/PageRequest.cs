namespace IkkonAdmin.Web.Infrastructure.Pagination;

public class PageRequest
{
    private static readonly int[] AllowedPageSizes = [20, 50, 100];

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Sort { get; set; }

    public void Normalize()
    {
        Page = Math.Max(1, Page);
        PageSize = AllowedPageSizes.Contains(PageSize) ? PageSize : 20;
        Sort = string.IsNullOrWhiteSpace(Sort) ? null : Sort.Trim().ToLowerInvariant();
    }
}
